using System;
using UnityEngine;

public class InteractionWithUI : MonoBehaviour
{
    [SerializeField] private PlayerInputReader _inputReader;
    [SerializeField] private bool _startInInterfaceMode;

    private bool _isInteracts;
    private bool _isSwitchAllowed = true;

    public bool IsInteracts => _isInteracts;
    public bool IsSwitchAllowed => _isSwitchAllowed;

    public event Action<bool> IsInteractsChanged;

    private void Awake()
    {
        if (_inputReader == null)
            _inputReader = GetComponent<PlayerInputReader>();

        SetInteracts(_startInInterfaceMode, true);
    }

    private void Start()
    {
        IsInteractsChanged?.Invoke(_isInteracts);
    }

    private void OnEnable()
    {
        if (_inputReader != null)
            _inputReader.OnToggleCursorRequested += SwitchMode;
    }

    private void OnDisable()
    {
        if (_inputReader != null)
            _inputReader.OnToggleCursorRequested -= SwitchMode;
    }

    public void SetSwitchAllowed(bool value)
    {
        _isSwitchAllowed = value;
    }

    public void SetInteracts(bool value)
    {
        SetInteracts(value, false);
    }

    private void SwitchMode()
    {
        if (!_isSwitchAllowed)
            return;

        SetInteracts(!_isInteracts);
    }

    private void SetInteracts(bool value, bool force)
    {
        if (!force && _isInteracts == value)
            return;

        _isInteracts = value;
        _inputReader?.SetInterfaceMode(value);
        IsInteractsChanged?.Invoke(_isInteracts);
    }
}
