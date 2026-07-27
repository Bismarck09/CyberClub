using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerInput))]
public class PlayerInputReader : MonoBehaviour
{
    [SerializeField] private PlayerInput _playerInput;

    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _sprintAction;
    private InputAction _interactAction;
    private InputAction _toggleCursorAction;
    private InputAction _pauseAction;

    private Vector2 _mobileMovement;
    private Vector2 _mobileLookDelta;
    private bool _isBlocked;
    private bool _isInterfaceMode;

    public Vector2 Movement => _isBlocked || _isInterfaceMode
        ? Vector2.zero
        : IsTouchMode
            ? _mobileMovement
            : _moveAction?.ReadValue<Vector2>() ?? Vector2.zero;

    public Vector2 Look
    {
        get
        {
            if (_isBlocked || _isInterfaceMode)
                return Vector2.zero;

            if (!IsTouchMode)
                return _lookAction?.ReadValue<Vector2>() ?? Vector2.zero;

            Vector2 value = _mobileLookDelta;
            _mobileLookDelta = Vector2.zero;
            return value;
        }
    }

    public bool Sprint => !_isBlocked && !_isInterfaceMode && !IsTouchMode &&
        _sprintAction?.IsPressed() == true;

    public bool Interact => !_isBlocked &&
        (_interactAction?.IsPressed() == true);

    public bool IsTouchMode { get; private set; }
    public bool IsGameplayInputAvailable => !_isBlocked && !_isInterfaceMode;
    public bool IsLookFromGamepad =>
        !IsTouchMode && _lookAction?.activeControl?.device is Gamepad;

    public event Action<bool> OnControlModeChanged;
    public event Action OnMobileControlsResetRequested;
    public event Action OnInteractRequested;
    public event Action OnToggleCursorRequested;
    public event Action OnPauseRequested;

    private void Awake()
    {
        if (_playerInput == null)
            _playerInput = GetComponent<PlayerInput>();

        if (_playerInput == null || _playerInput.actions == null)
            return;

        _moveAction = _playerInput.actions.FindAction("Move", false);
        _lookAction = _playerInput.actions.FindAction("Look", false);
        _sprintAction = _playerInput.actions.FindAction("Sprint", false);
        _interactAction = _playerInput.actions.FindAction("Interact", false);
        _toggleCursorAction = _playerInput.actions.FindAction("InteractionWithInterface", false);
        _pauseAction = _playerInput.actions.FindAction("Pause", false);
    }

    private void OnEnable()
    {
        Subscribe(_interactAction, HandleInteract);
        Subscribe(_toggleCursorAction, HandleToggleCursor);
        Subscribe(_pauseAction, HandlePause);
    }

    private void OnDisable()
    {
        Unsubscribe(_interactAction, HandleInteract);
        Unsubscribe(_toggleCursorAction, HandleToggleCursor);
        Unsubscribe(_pauseAction, HandlePause);
        ResetMobileState();
    }

    private void Update()
    {
        bool hasTouchInput = false;

        if (Touchscreen.current != null)
        {
            foreach (TouchControl touch in Touchscreen.current.touches)
            {
                if (touch.press.isPressed ||
                    touch.press.wasPressedThisFrame ||
                    touch.press.wasReleasedThisFrame)
                {
                    hasTouchInput = true;
                    break;
                }
            }
        }

        if (hasTouchInput)
        {
            SetTouchMode(true);
            return;
        }

        bool hasDesktopInput =
            Keyboard.current?.anyKey.wasPressedThisFrame == true ||
            Mouse.current?.leftButton.wasPressedThisFrame == true ||
            Mouse.current?.rightButton.wasPressedThisFrame == true ||
            (Mouse.current?.delta.ReadValue().sqrMagnitude ?? 0f) > 0.01f;

        if (hasDesktopInput)
            SetTouchMode(false);
    }

    public void SetBlocked(bool value)
    {
        if (_isBlocked == value)
            return;

        _isBlocked = value;

        if (value)
        {
            ResetMobileState();
            OnMobileControlsResetRequested?.Invoke();
        }
    }

    public void SetInterfaceMode(bool value)
    {
        if (_isInterfaceMode == value)
            return;

        _isInterfaceMode = value;

        if (value)
        {
            ResetMobileState();
            OnMobileControlsResetRequested?.Invoke();
        }
    }

    public void SetMobileMovement(Vector2 value)
    {
        _mobileMovement = IsGameplayInputAvailable
            ? Vector2.ClampMagnitude(value, 1f)
            : Vector2.zero;
    }

    public void ResetMobileMovement()
    {
        _mobileMovement = Vector2.zero;
    }

    public void AddMobileLookDelta(Vector2 value)
    {
        if (IsGameplayInputAvailable)
            _mobileLookDelta += value;
    }

    public void ResetMobileLook()
    {
        _mobileLookDelta = Vector2.zero;
    }

    public void ResetMobileState()
    {
        ResetMobileMovement();
        ResetMobileLook();
    }

    private void SetTouchMode(bool value)
    {
        if (IsTouchMode == value)
            return;

        IsTouchMode = value;
        ResetMobileState();
        OnControlModeChanged?.Invoke(value);
    }

    private void HandleInteract(InputAction.CallbackContext unusedContext)
    {
        if (!_isBlocked)
            OnInteractRequested?.Invoke();
    }

    private void HandleToggleCursor(InputAction.CallbackContext unusedContext)
    {
        OnToggleCursorRequested?.Invoke();
    }

    private void HandlePause(InputAction.CallbackContext unusedContext)
    {
        OnPauseRequested?.Invoke();
    }

    private static void Subscribe(
        InputAction action,
        Action<InputAction.CallbackContext> handler)
    {
        if (action != null)
            action.performed += handler;
    }

    private static void Unsubscribe(
        InputAction action,
        Action<InputAction.CallbackContext> handler)
    {
        if (action != null)
            action.performed -= handler;
    }
}
