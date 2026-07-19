using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialInputBlocker : MonoBehaviour
{
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private PlayerInputReader _inputReader;

    [Header("Disable only gameplay actions")]
    [SerializeField] private string[] _blockedActionNames = { "Move", "Look" };

    private InputAction[] _blockedActions;
    private bool _isBlocked;

    public bool IsBlocked => _isBlocked;

    private void Awake()
    {
        CacheActions();
    }

    private void OnDisable()
    {
        SetBlocked(false);
    }

    public void SetBlocked(bool value)
    {
        if (_isBlocked == value)
            return;

        _isBlocked = value;
        _inputReader?.SetBlocked(value);

        if (_blockedActions == null || _blockedActions.Length == 0)
            CacheActions();

        foreach (InputAction action in _blockedActions)
        {
            if (action == null)
                continue;

            if (_isBlocked)
                action.Disable();
            else
                action.Enable();
        }
    }

    private void CacheActions()
    {
        if (_playerInput == null || _playerInput.actions == null)
            return;

        _blockedActions = new InputAction[_blockedActionNames.Length];

        for (int i = 0; i < _blockedActionNames.Length; i++)
            _blockedActions[i] = _playerInput.actions.FindAction(_blockedActionNames[i], false);
    }
}
