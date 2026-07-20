using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CursorActive : MonoBehaviour
{
    [SerializeField] private InteractionWithUI _interactionWithUI;
    [SerializeField] private PlayerInputReader _inputReader;

    [Header("Authored pointer-lock UI")]
    [SerializeField] private GameObject _startOverlay;
    [SerializeField] private Button _startOverlayButton;

    private bool _lastInterfaceState;
    private bool _hasStartGesture;
    private float _pointerLockCheckTime;

    private void Awake()
    {
        if (_inputReader == null)
            _inputReader = GetComponentInParent<PlayerInputReader>();

        ValidateReferences();
    }

    private void OnEnable()
    {
        if (_interactionWithUI != null)
            _interactionWithUI.IsInteractsChanged += SwitchCursorActive;

        if (_inputReader != null)
            _inputReader.OnControlModeChanged += HandleControlModeChanged;

        if (_startOverlayButton != null)
            _startOverlayButton.onClick.AddListener(AcceptStartGesture);

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

        if (_startOverlayButton != null)
            _startOverlayButton.onClick.RemoveListener(AcceptStartGesture);
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

    private static bool RequiresBrowserGesture()
    {
        return Application.platform == RuntimePlatform.WebGLPlayer;
    }

    private void ShowStartOverlay()
    {
        if (_startOverlay == null)
        {
            ReportMissing(nameof(_startOverlay));
            return;
        }

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

    private void ValidateReferences()
    {
        if (_interactionWithUI == null)
            ReportMissing(nameof(_interactionWithUI));
        if (_inputReader == null)
            ReportMissing(nameof(_inputReader));
        if (_startOverlay == null)
            ReportMissing(nameof(_startOverlay));
        if (_startOverlayButton == null)
            ReportMissing(nameof(_startOverlayButton));
    }

    private void ReportMissing(string fieldName)
    {
        Debug.LogError($"CursorActive: поле {fieldName} не назначено на GameObject '{name}'.", this);
    }
}
