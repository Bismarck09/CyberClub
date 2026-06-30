using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionWithUI : MonoBehaviour
{
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private string _actionName = "InteractionWithInterface";

    private InputAction _interactAction;
    private bool _isInteracts;

    public bool IsInteracts => _isInteracts;

    public event Action<bool> IsInteractsChanged;

    private void Awake()
    {
        if (_playerInput == null)
            _playerInput = GetComponent<PlayerInput>();

        if (_playerInput != null)
            _interactAction = _playerInput.actions[_actionName];

        SetInteracts(false, true);
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

    public void SetInteracts(bool value)
    {
        SetInteracts(value, false);
    }

    public void Toggle()
    {
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