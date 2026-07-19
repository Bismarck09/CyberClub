using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MobileControlsHUD : MonoBehaviour
{
    [SerializeField] private PlayerInputReader _inputReader;
    [SerializeField, Min(0.01f)] private float _lookScale = 0.12f;

    private readonly HashSet<int> _controlPointerIds = new();
    private GameObject _visualRoot;
    private GameObject _pauseOverlay;
    private RectTransform _safeArea;
    private Rect _lastSafeArea;
    private bool _isPaused;
    private bool _isHudVisible;
    private bool _isChangingHudVisibility;
    private float _timeScaleBeforePause = 1f;

    private void Awake()
    {
        if (_inputReader == null)
            _inputReader = GetComponent<PlayerInputReader>();

        BuildRuntimeHud();
        ApplySafeArea();
        SetHudVisible(_inputReader != null && _inputReader.IsTouchMode);
    }

    private void OnEnable()
    {
        if (_inputReader == null)
            return;

        _inputReader.OnControlModeChanged += SetHudVisible;
        _inputReader.OnPauseRequested += TogglePause;
    }

    private void OnDisable()
    {
        if (_inputReader != null)
        {
            _inputReader.OnControlModeChanged -= SetHudVisible;
            _inputReader.OnPauseRequested -= TogglePause;
            _inputReader.ResetMobileState();
        }

        if (_isPaused)
            SetPaused(false);
    }

    private void OnDestroy()
    {
        if (_visualRoot != null)
            Destroy(_visualRoot);
    }

    private void Update()
    {
        if (_lastSafeArea != Screen.safeArea)
            ApplySafeArea();

        if (_inputReader == null || !_inputReader.IsTouchMode || _isPaused)
            return;

        ReadLookTouches();
    }

    public void RegisterControlPointer(int pointerId)
    {
        _inputReader?.RegisterTouchActivity();
        _controlPointerIds.Add(pointerId);
    }

    public void UnregisterControlPointer(int pointerId)
    {
        _controlPointerIds.Remove(pointerId);
    }

    private void ReadLookTouches()
    {
        if (Touchscreen.current == null)
            return;

        foreach (TouchControl touch in Touchscreen.current.touches)
        {
            if (!touch.press.isPressed)
                continue;

            int touchId = touch.touchId.ReadValue();
            Vector2 position = touch.position.ReadValue();

            if (_controlPointerIds.Contains(touchId) ||
                position.x < Screen.width * 0.45f)
            {
                continue;
            }

            // ИЗМЕНЕНО: ScrollView, кнопки и любые другие UI-элементы
            // получают touch без одновременного вращения камеры.
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject(touchId))
            {
                continue;
            }

            Vector2 delta = touch.delta.ReadValue();

            if (delta.sqrMagnitude > 0f)
                _inputReader.AddMobileLookDelta(delta * _lookScale);
        }
    }

    private void SetHudVisible(bool isTouchMode)
    {
        if (_visualRoot == null || _isChangingHudVisibility)
        {
            return;
        }

        if (_isHudVisible == isTouchMode && _visualRoot.activeSelf == isTouchMode)
            return;

        _isChangingHudVisibility = true;

        try
        {
            if (!isTouchMode)
            {
                // ИЗМЕНЕНО: состояние ввода обнуляется до деактивации дочерних
                // контролов; их OnDisable больше не переключает input mode.
                _controlPointerIds.Clear();
                _inputReader?.ResetMobileState();

                if (_isPaused)
                    SetPaused(false);
            }

            if (_visualRoot.activeSelf != isTouchMode)
                _visualRoot.SetActive(isTouchMode);

            _isHudVisible = isTouchMode;
        }
        finally
        {
            _isChangingHudVisibility = false;
        }
    }

    private void TogglePause()
    {
        SetPaused(!_isPaused);
    }

    private void SetPaused(bool value)
    {
        if (_isPaused == value)
            return;

        _isPaused = value;

        if (value)
        {
            _timeScaleBeforePause = Time.timeScale > 0f ? Time.timeScale : 1f;
            Time.timeScale = 0f;
            _inputReader?.ResetMobileState();
        }
        else
        {
            Time.timeScale = _timeScaleBeforePause;
        }

        if (_pauseOverlay != null)
            _pauseOverlay.SetActive(value);
    }

    private void BuildRuntimeHud()
    {
        GameObject canvasObject = new GameObject(
            "MobileControlsHUD_Runtime",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        // ИЗМЕНЕНО: ScreenSpaceOverlay Canvas остаётся корневым объектом,
        // чтобы масштаб/поворот игрока не влияли на touch HUD.
        canvasObject.transform.SetParent(null, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 40;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        _visualRoot = canvasObject;
        _isHudVisible = true;
        _safeArea = CreateRect("SafeArea", canvasObject.transform);

        CreateJoystick();
        CreateTouchButton("Взаимодействие", new Vector2(-170f, 270f), new Vector2(250f, 100f),
            () => _inputReader?.SubmitMobileInteract());
        CreateTouchButton("Меню", new Vector2(-170f, 150f), new Vector2(250f, 100f),
            () => _inputReader?.SubmitMobileToggleCursor());
        CreateTouchButton("Пауза", new Vector2(-170f, -60f), new Vector2(220f, 90f),
            () => _inputReader?.SubmitMobilePause(), true);
        CreateTouchButton("Бег", new Vector2(-430f, 150f), new Vector2(180f, 100f),
            () => _inputReader?.SetMobileSprint(true),
            false,
            () => _inputReader?.SetMobileSprint(false));

        CreatePauseOverlay();
    }

    private void CreateJoystick()
    {
        RectTransform background = CreateImage(
            "MovementJoystick",
            _safeArea,
            new Color(0.05f, 0.08f, 0.12f, 0.58f));

        background.anchorMin = Vector2.zero;
        background.anchorMax = Vector2.zero;
        background.pivot = new Vector2(0.5f, 0.5f);
        background.sizeDelta = new Vector2(260f, 260f);
        background.anchoredPosition = new Vector2(190f, 190f);

        RectTransform handle = CreateImage(
            "Handle",
            background,
            new Color(0.25f, 0.75f, 1f, 0.85f));

        handle.anchorMin = handle.anchorMax = new Vector2(0.5f, 0.5f);
        handle.sizeDelta = new Vector2(105f, 105f);

        MobileVirtualJoystick joystick = background.gameObject.AddComponent<MobileVirtualJoystick>();
        joystick.Initialize(_inputReader, this, background, handle);
    }

    private void CreateTouchButton(
        string label,
        Vector2 anchoredPosition,
        Vector2 size,
        Action onDown,
        bool topAnchored = false,
        Action onUp = null)
    {
        RectTransform buttonRect = CreateImage(
            label,
            _safeArea,
            new Color(0.08f, 0.12f, 0.18f, 0.78f));

        buttonRect.anchorMin = buttonRect.anchorMax = topAnchored
            ? new Vector2(1f, 1f)
            : new Vector2(1f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = size;
        buttonRect.anchoredPosition = anchoredPosition;

        TextMeshProUGUI text = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI))
            .GetComponent<TextMeshProUGUI>();
        text.transform.SetParent(buttonRect, false);
        RectTransform textRect = (RectTransform)text.transform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = textRect.offsetMax = Vector2.zero;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 28f;
        text.text = label;
        text.raycastTarget = false;

        MobileTouchButton button = buttonRect.gameObject.AddComponent<MobileTouchButton>();
        button.Initialize(this, onDown, onUp);
    }

    private void CreatePauseOverlay()
    {
        RectTransform overlay = CreateImage(
            "PauseOverlay",
            _safeArea,
            new Color(0f, 0f, 0f, 0.72f));

        overlay.anchorMin = Vector2.zero;
        overlay.anchorMax = Vector2.one;
        overlay.offsetMin = overlay.offsetMax = Vector2.zero;
        _pauseOverlay = overlay.gameObject;

        RectTransform resume = CreateImage(
            "Продолжить",
            overlay,
            new Color(0.1f, 0.55f, 0.75f, 0.95f));
        resume.anchorMin = resume.anchorMax = new Vector2(0.5f, 0.5f);
        resume.sizeDelta = new Vector2(360f, 120f);

        TextMeshProUGUI label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI))
            .GetComponent<TextMeshProUGUI>();
        label.transform.SetParent(resume, false);
        RectTransform labelRect = (RectTransform)label.transform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 36f;
        label.text = "Продолжить";
        label.raycastTarget = false;

        MobileTouchButton button = resume.gameObject.AddComponent<MobileTouchButton>();
        button.Initialize(this, () => SetPaused(false), null);
        _pauseOverlay.SetActive(false);
    }

    private void ApplySafeArea()
    {
        if (_safeArea == null || Screen.width <= 0 || Screen.height <= 0)
            return;

        Rect safe = Screen.safeArea;
        _lastSafeArea = safe;
        _safeArea.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
        _safeArea.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
        _safeArea.offsetMin = _safeArea.offsetMax = Vector2.zero;
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return rect;
    }

    private static RectTransform CreateImage(string name, Transform parent, Color color)
    {
        Image image = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        image.transform.SetParent(parent, false);
        image.color = color;
        return (RectTransform)image.transform;
    }
}

internal sealed class MobileVirtualJoystick : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    private PlayerInputReader _reader;
    private MobileControlsHUD _hud;
    private RectTransform _background;
    private RectTransform _handle;
    private int _pointerId = int.MinValue;

    public void Initialize(
        PlayerInputReader reader,
        MobileControlsHUD hud,
        RectTransform background,
        RectTransform handle)
    {
        _reader = reader;
        _hud = hud;
        _background = background;
        _handle = handle;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_pointerId != int.MinValue)
            return;

        _pointerId = eventData.pointerId;
        _hud.RegisterControlPointer(_pointerId);
        UpdateValue(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId == _pointerId)
            UpdateValue(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != _pointerId)
            return;

        _hud.UnregisterControlPointer(_pointerId);
        _pointerId = int.MinValue;
        _handle.anchoredPosition = Vector2.zero;
        _reader?.ResetMobileMovement();
    }

    private void OnDisable()
    {
        if (_pointerId != int.MinValue)
            _hud?.UnregisterControlPointer(_pointerId);

        _pointerId = int.MinValue;

        if (_handle != null)
            _handle.anchoredPosition = Vector2.zero;

        _reader?.ResetMobileMovement();
    }

    private void UpdateValue(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _background,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
        {
            return;
        }

        float radius = Mathf.Max(1f, _background.rect.width * 0.5f);
        Vector2 value = Vector2.ClampMagnitude(localPoint / radius, 1f);
        _handle.anchoredPosition = value * radius * 0.62f;
        _reader?.SetMobileMovement(value);
    }
}

internal sealed class MobileTouchButton : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    private MobileControlsHUD _hud;
    private Action _onDown;
    private Action _onUp;
    private int _pointerId = int.MinValue;

    public void Initialize(MobileControlsHUD hud, Action onDown, Action onUp)
    {
        _hud = hud;
        _onDown = onDown;
        _onUp = onUp;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_pointerId != int.MinValue)
            return;

        _pointerId = eventData.pointerId;
        _hud.RegisterControlPointer(_pointerId);
        _onDown?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Release(eventData.pointerId);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Release(eventData.pointerId);
    }

    private void OnDisable()
    {
        if (_pointerId == int.MinValue)
            return;

        _hud?.UnregisterControlPointer(_pointerId);
        _pointerId = int.MinValue;
        _onUp?.Invoke();
    }

    private void Release(int pointerId)
    {
        if (pointerId != _pointerId)
            return;

        _hud.UnregisterControlPointer(_pointerId);
        _pointerId = int.MinValue;
        _onUp?.Invoke();
    }
}
