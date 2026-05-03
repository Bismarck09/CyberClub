using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InteractionWithUI : MonoBehaviour
{
    [SerializeField] private PlayerInput _playerInput;
    private InputAction _interactAction;
    private bool _isInteracts;

    public event Action<bool> IsInteractsChanged;

    private void Awake()
    {
        //_playerInput = GetComponent<PlayerInput>();
        _interactAction = _playerInput.actions["InteractionWithInterface"];
        
        _isInteracts = false;
    }

    private void Start()
    {
        IsInteractsChanged?.Invoke(_isInteracts);        
    }

    private void OnEnable() => _interactAction.performed += SwitchMode;
    private void OnDisable() => _interactAction.performed -= SwitchMode;


    private void SwitchMode(InputAction.CallbackContext context)
    {
        _isInteracts = !_isInteracts;
        IsInteractsChanged?.Invoke(_isInteracts);
    }
}
