using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public enum JoystickPlacementMode
{
    Fixed,
    Floating
}

[DisallowMultipleComponent]
public sealed class MobileVirtualJoystick : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private RectTransform _background;
    [SerializeField] private RectTransform _handle;

    [Header("Placement")]
    [SerializeField] private JoystickPlacementMode _placementMode =
        JoystickPlacementMode.Floating;
    [SerializeField] private Rect _activationArea = new(0f, 0f, 0.4f, 1f);
    [SerializeField, Min(24f)] private float _radius = 120f;
    [SerializeField, Range(0f, 0.95f)] private float _deadZone = 0.12f;
    [SerializeField] private bool _hideWhenReleased = true;
    [SerializeField] private bool _clampBaseToScreen = true;
    [SerializeField, Min(0f)] private float _edgePadding = 24f;
    [SerializeField, Min(0f)] private float _returnDuration = 0.12f;

    private const int NoPointer = int.MinValue;

    private PlayerInputReader _inputReader;
    private RectTransform _root;
    private Canvas _canvas;
    private TouchControl _activeTouch;
    private int _pointerId = NoPointer;
    private Vector2 _fixedAnchoredPosition;
    private Vector2 _returnBaseStart;
    private Vector2 _returnHandleStart;
    private float _returnElapsed;
    private bool _isReturning;

    public int ActivePointerId => _pointerId;
    public bool HasActivePointer => _activeTouch != null;

    private void Awake()
    {
        _root = transform as RectTransform;
        _canvas = GetComponentInParent<Canvas>();

        if (_root != null)
            _fixedAnchoredPosition = _root.anchoredPosition;

        DisableTechnicalRaycasts();
        ApplyVisualDimensions();

        if (_background == null)
            ReportMissing(nameof(_background));
        if (_handle == null)
            ReportMissing(nameof(_handle));

        ResetImmediate();
    }

    private void Update()
    {
        if (_activeTouch != null)
        {
            UpdateCapturedTouch();
            return;
        }

        UpdateReturnAnimation();

        if (_isReturning ||
            _inputReader == null ||
            !_inputReader.IsTouchMode ||
            !_inputReader.IsGameplayInputAvailable)
        {
            return;
        }

        TryCaptureNewTouch();
    }

    public void Bind(PlayerInputReader inputReader)
    {
        _inputReader = inputReader;
    }

    public bool IsPointerCaptured(int pointerId)
    {
        return _pointerId != NoPointer && _pointerId == pointerId;
    }

    public void ResetControl()
    {
        _inputReader?.ResetMobileMovement();

        if (_activeTouch == null && !_isReturning)
            return;

        _activeTouch = null;
        _pointerId = NoPointer;
        StartReturnAnimation();
    }

    private void OnDisable()
    {
        ResetImmediate();
    }

    private void TryCaptureNewTouch()
    {
        Touchscreen touchscreen = Touchscreen.current;
        if (touchscreen == null)
            return;

        foreach (TouchControl touch in touchscreen.touches)
        {
            if (!touch.press.wasPressedThisFrame)
                continue;

            Vector2 screenPosition = touch.position.ReadValue();
            int pointerId = touch.touchId.ReadValue();

            if (!IsInsideActivationArea(screenPosition) ||
                MobilePointerUiGuard.IsPointerOverInteractiveUi(
                    screenPosition,
                    pointerId,
                    transform))
            {
                continue;
            }

            CaptureTouch(touch, pointerId, screenPosition);
            return;
        }
    }

    private void CaptureTouch(
        TouchControl touch,
        int pointerId,
        Vector2 screenPosition)
    {
        _activeTouch = touch;
        _pointerId = pointerId;
        _isReturning = false;

        if (_placementMode == JoystickPlacementMode.Floating)
            PlaceBaseAt(screenPosition);

        SetVisualsVisible(true);
        UpdateValue(screenPosition);
    }

    private void UpdateCapturedTouch()
    {
        if (_inputReader == null ||
            !_inputReader.IsGameplayInputAvailable ||
            !_activeTouch.press.isPressed)
        {
            ResetControl();
            return;
        }

        UpdateValue(_activeTouch.position.ReadValue());
    }

    private void UpdateValue(Vector2 screenPosition)
    {
        if (_background == null || _handle == null)
            return;

        Camera eventCamera = GetEventCamera();
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _background,
                screenPosition,
                eventCamera,
                out Vector2 localPoint))
        {
            return;
        }

        Vector2 rawOffset = localPoint - _background.rect.center;
        Vector2 clampedOffset = Vector2.ClampMagnitude(rawOffset, _radius);
        float handleRadius = Mathf.Max(
            0f,
            _radius - Mathf.Min(_handle.rect.width, _handle.rect.height) * 0.5f);
        _handle.anchoredPosition = Vector2.ClampMagnitude(rawOffset, handleRadius);

        float magnitude = Mathf.Clamp01(clampedOffset.magnitude / _radius);
        Vector2 value = Vector2.zero;

        if (magnitude > _deadZone)
        {
            float remappedMagnitude =
                (magnitude - _deadZone) / Mathf.Max(0.0001f, 1f - _deadZone);
            value = clampedOffset.normalized * remappedMagnitude;
        }

        _inputReader?.SetMobileMovement(value);
    }

    private bool IsInsideActivationArea(Vector2 screenPosition)
    {
        if (_placementMode == JoystickPlacementMode.Fixed)
        {
            return _root != null && RectTransformUtility.RectangleContainsScreenPoint(
                _root,
                screenPosition,
                GetEventCamera());
        }

        return GetActivationScreenRect().Contains(screenPosition);
    }

    private void PlaceBaseAt(Vector2 requestedScreenPosition)
    {
        if (_root == null || _root.parent is not RectTransform parent)
            return;

        Vector2 screenPosition = _clampBaseToScreen
            ? ClampBaseScreenPosition(requestedScreenPosition)
            : requestedScreenPosition;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                parent,
                screenPosition,
                GetEventCamera(),
                out Vector3 worldPoint))
        {
            _root.position = worldPoint;
        }
    }

    private Vector2 ClampBaseScreenPosition(Vector2 position)
    {
        Rect available = IntersectRects(GetActivationScreenRect(), Screen.safeArea);
        float scale = _canvas != null ? Mathf.Max(0.01f, _canvas.scaleFactor) : 1f;
        float inset = (_radius + _edgePadding) * scale;

        float minX = available.xMin + inset;
        float maxX = available.xMax - inset;
        float minY = available.yMin + inset;
        float maxY = available.yMax - inset;

        if (minX > maxX)
            minX = maxX = available.center.x;
        if (minY > maxY)
            minY = maxY = available.center.y;

        return new Vector2(
            Mathf.Clamp(position.x, minX, maxX),
            Mathf.Clamp(position.y, minY, maxY));
    }

    private Rect GetActivationScreenRect()
    {
        float xMin = Mathf.Clamp01(_activationArea.xMin) * Screen.width;
        float yMin = Mathf.Clamp01(_activationArea.yMin) * Screen.height;
        float xMax = Mathf.Clamp01(_activationArea.xMax) * Screen.width;
        float yMax = Mathf.Clamp01(_activationArea.yMax) * Screen.height;

        return Rect.MinMaxRect(
            Mathf.Min(xMin, xMax),
            Mathf.Min(yMin, yMax),
            Mathf.Max(xMin, xMax),
            Mathf.Max(yMin, yMax));
    }

    private void StartReturnAnimation()
    {
        if (_root == null || _handle == null || _returnDuration <= 0f)
        {
            FinishReturn();
            return;
        }

        _returnBaseStart = _root.anchoredPosition;
        _returnHandleStart = _handle.anchoredPosition;
        _returnElapsed = 0f;
        _isReturning = true;
    }

    private void UpdateReturnAnimation()
    {
        if (!_isReturning || _root == null || _handle == null)
            return;

        _returnElapsed += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(_returnElapsed / Mathf.Max(0.0001f, _returnDuration));
        float smoothT = t * t * (3f - 2f * t);

        _handle.anchoredPosition = Vector2.Lerp(_returnHandleStart, Vector2.zero, smoothT);

        if (_placementMode == JoystickPlacementMode.Floating && !_hideWhenReleased)
        {
            _root.anchoredPosition = Vector2.Lerp(
                _returnBaseStart,
                _fixedAnchoredPosition,
                smoothT);
        }

        if (t >= 1f)
            FinishReturn();
    }

    private void FinishReturn()
    {
        _isReturning = false;

        if (_handle != null)
            _handle.anchoredPosition = Vector2.zero;

        if (_root != null &&
            _placementMode == JoystickPlacementMode.Floating &&
            !_hideWhenReleased)
        {
            _root.anchoredPosition = _fixedAnchoredPosition;
        }

        SetVisualsVisible(!_hideWhenReleased);
    }

    private void ResetImmediate()
    {
        _activeTouch = null;
        _pointerId = NoPointer;
        _isReturning = false;
        _inputReader?.ResetMobileMovement();

        if (_handle != null)
            _handle.anchoredPosition = Vector2.zero;

        if (_root != null && _placementMode == JoystickPlacementMode.Floating)
            _root.anchoredPosition = _fixedAnchoredPosition;

        SetVisualsVisible(!_hideWhenReleased);
    }

    private void SetVisualsVisible(bool visible)
    {
        if (_background != null && _background.gameObject.activeSelf != visible)
            _background.gameObject.SetActive(visible);
        if (_handle != null && _handle.gameObject.activeSelf != visible)
            _handle.gameObject.SetActive(visible);
    }

    private void ApplyVisualDimensions()
    {
        if (_root != null)
            _root.sizeDelta = Vector2.one * (_radius * 2f);
        if (_background != null)
            _background.sizeDelta = Vector2.one * (_radius * 2f);
    }

    private void DisableTechnicalRaycasts()
    {
        Graphic[] graphics = GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
            graphics[i].raycastTarget = false;
    }

    private Camera GetEventCamera()
    {
        if (_canvas == null || _canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        return _canvas.worldCamera;
    }

    private static Rect IntersectRects(Rect first, Rect second)
    {
        float xMin = Mathf.Max(first.xMin, second.xMin);
        float yMin = Mathf.Max(first.yMin, second.yMin);
        float xMax = Mathf.Min(first.xMax, second.xMax);
        float yMax = Mathf.Min(first.yMax, second.yMax);

        return xMax > xMin && yMax > yMin
            ? Rect.MinMaxRect(xMin, yMin, xMax, yMax)
            : second;
    }

    private void ReportMissing(string fieldName)
    {
        Debug.LogError(
            $"MobileVirtualJoystick: поле {fieldName} не назначено на GameObject '{name}'.",
            this);
    }
}
