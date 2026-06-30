using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionWithUI : MonoBehaviour
{
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private string _actionName = "InteractionWithInterface";
    [SerializeField] private bool _startInInterfaceMode;

    private InputAction _interactAction;
    private bool _isInteracts;
    private bool _isModeSwitchAllowed = true;

    public bool IsInteracts => _isInteracts;
    public bool IsModeSwitchAllowed => _isModeSwitchAllowed;

    public event Action<bool> IsInteractsChanged;

    private void Awake()
    {
        if (_playerInput == null)
            _playerInput = GetComponent<PlayerInput>();

        if (_playerInput != null && _playerInput.actions != null)
            _interactAction = _playerInput.actions.FindAction(_actionName, false);

        SetInteracts(_startInInterfaceMode, true);
    }

    private void Start()
    {
        IsInteractsChanged?.Invoke(_isInteracts);
    }

    private void OnEnable()
    {
        if (_interactAction != null)
            _interactAction.performed += SwitchMode;
    }

    private void OnDisable()
    {
        if (_interactAction != null)
            _interactAction.performed -= SwitchMode;
    }

    public void SetModeSwitchAllowed(bool value)
    {
        _isModeSwitchAllowed = value;
    }

    public void SetInteracts(bool value)
    {
        SetInteracts(value, false);
    }

    public void Toggle()
    {
        if (!_isModeSwitchAllowed)
            return;

        SetInteracts(!_isInteracts);
    }

    private void SwitchMode(InputAction.CallbackContext context)
    {
        Toggle();
    }

    private void SetInteracts(bool value, bool force)
    {
        if (!force && _isInteracts == value)
            return;

        _isInteracts = value;
        IsInteractsChanged?.Invoke(_isInteracts);
    }
}