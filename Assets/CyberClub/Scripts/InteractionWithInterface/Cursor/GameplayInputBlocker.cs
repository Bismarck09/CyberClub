using UnityEngine;
using UnityEngine.InputSystem;

public class GameplayInputBlocker : MonoBehaviour
{
    [SerializeField] private PlayerInput _playerInput;

    [Header("Actions blocked during tutorial")]
    [SerializeField] private string[] _actionNamesToDisable =
    {
        "Move",
        "Look"
    };

    private InputAction[] _actions;
    private bool _isBlocked;

    public bool IsBlocked => _isBlocked;

    private void Awake()
    {
        if (_playerInput == null)
            _playerInput = FindFirstObjectByType<PlayerInput>();

        CacheActions();
    }

    public void SetBlocked(bool value)
    {
        if (_isBlocked == value)
            return;

        _isBlocked = value;

        if (_actions == null || _actions.Length == 0)
            CacheActions();

        foreach (InputAction action in _actions)
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

        _actions = new InputAction[_actionNamesToDisable.Length];

        for (int i = 0; i < _actionNamesToDisable.Length; i++)
        {
            string actionName = _actionNamesToDisable[i];
            _actions[i] = _playerInput.actions.FindAction(actionName, false);
        }
    }
}