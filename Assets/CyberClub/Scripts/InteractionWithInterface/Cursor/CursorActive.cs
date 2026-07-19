using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class CursorActive : MonoBehaviour
{
    [SerializeField] private InteractionWithUI _interactionWithUI;
    [SerializeField] private PlayerInputReader _inputReader;
    [SerializeField] private Canvas _overlayCanvas;

    private GameObject _startOverlay;
    private bool _lastInterfaceState;
    private bool _hasStartGesture;
    private float _pointerLockCheckTime;

    private void Awake()
    {
        if (_inputReader == null)
            _inputReader = GetComponentInParent<PlayerInputReader>();
    }

    private void OnEnable()
    {
        if (_interactionWithUI != null)
            _interactionWithUI.IsInteractsChanged += SwitchCursorActive;

        if (_inputReader != null)
            _inputReader.OnControlModeChanged += HandleControlModeChanged;

        if (_interactionWithUI != null)
        {
            _lastInterfaceState = _interactionWithUI.IsInteracts;
            SwitchCursorActive(_lastInterfaceState);
        }
    }

    private void OnDisable()
    {
        if (_interactionWithUI != null)
            _interactionWithUI.IsInteractsChanged -= SwitchCursorActive;

        if (_inputReader != null)
            _inputReader.OnControlModeChanged -= HandleControlModeChanged;
    }

    private void Update()
    {
        if (_startOverlay == null || !_startOverlay.activeSelf)
            return;

        bool pressed =
            (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        if (pressed)
            AcceptStartGesture();
    }

    private void SwitchCursorActive(bool isActive)
    {
        if (_inputReader != null && _inputReader.IsTouchMode)
        {
            HideStartOverlay();
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.None;
            _lastInterfaceState = isActive;
            return;
        }

        if (isActive)
        {
            HideStartOverlay();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            _lastInterfaceState = true;
            return;
        }

        bool transitionedFromInterface = _lastInterfaceState;
        _lastInterfaceState = false;

        if (RequiresBrowserGesture() && !_hasStartGesture && !transitionedFromInterface)
        {
            ShowStartOverlay();
            return;
        }

        if (transitionedFromInterface)
            _hasStartGesture = true;

        HideStartOverlay();
        RequestGameplayCursorLock();
    }

    private void HandleControlModeChanged(bool unusedIsTouchMode)
    {
        SwitchCursorActive(_interactionWithUI != null && _interactionWithUI.IsInteracts);
    }

    private void AcceptStartGesture()
    {
        _hasStartGesture = true;
        HideStartOverlay();
        RequestGameplayCursorLock();
    }

    private void RequestGameplayCursorLock()
    {
        if (_inputReader != null && _inputReader.IsTouchMode)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.None;
            return;
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        _inputReader?.SetInterfaceMode(false);
        _pointerLockCheckTime = Time.unscaledTime + 0.5f;
    }

    private void LateUpdate()
    {
        if (!RequiresBrowserGesture() ||
            _lastInterfaceState ||
            (_inputReader != null && _inputReader.IsTouchMode) ||
            Time.unscaledTime < _pointerLockCheckTime ||
            (_startOverlay != null && _startOverlay.activeSelf))
        {
            return;
        }

        if (Cursor.lockState != CursorLockMode.Locked)
        {
            _hasStartGesture = false;
            ShowStartOverlay();
        }
    }

    private bool RequiresBrowserGesture()
    {
        return Application.platform == RuntimePlatform.WebGLPlayer;
    }

    private void ShowStartOverlay()
    {
        EnsureStartOverlay();

        if (_startOverlay != null)
            _startOverlay.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        _inputReader?.SetInterfaceMode(true);
    }

    private void HideStartOverlay()
    {
        if (_startOverlay != null)
            _startOverlay.SetActive(false);
    }

    private void EnsureStartOverlay()
    {
        if (_startOverlay != null || _overlayCanvas == null)
            return;

        _startOverlay = new GameObject("PointerLockStartOverlay", typeof(RectTransform));
        _startOverlay.layer = _overlayCanvas.gameObject.layer;
        _startOverlay.transform.SetParent(_overlayCanvas.transform, false);

        RectTransform rect = (RectTransform)_startOverlay.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = _startOverlay.AddComponent<Image>();
        image.color = new Color(0.015f, 0.02f, 0.035f, 0.9f);
        Button button = _startOverlay.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(AcceptStartGesture);

        GameObject labelObject = new GameObject("Label", typeof(RectTransform));
        labelObject.layer = _startOverlay.layer;
        labelObject.transform.SetParent(_startOverlay.transform, false);

        RectTransform labelRect = (RectTransform)labelObject.transform;
        labelRect.anchorMin = new Vector2(0.15f, 0.4f);
        labelRect.anchorMax = new Vector2(0.85f, 0.6f);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.text = "Нажмите, чтобы начать";
        label.fontSize = 44f;
        label.enableAutoSizing = true;
        label.fontSizeMin = 22f;
        label.fontSizeMax = 44f;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;
    }
}
