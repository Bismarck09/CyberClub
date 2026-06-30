using UnityEngine;

public class CursorActive : MonoBehaviour
{
    [SerializeField] private InteractionWithUI _interactionWithUI;

    private void Awake()
    {
        if (_interactionWithUI == null)
            _interactionWithUI = FindAnyObjectByType<InteractionWithUI>();
    }

    private void OnEnable()
    {
        if (_interactionWithUI != null)
            _interactionWithUI.IsInteractsChanged += SwitchCursorActive;

        if (_interactionWithUI != null)
            SwitchCursorActive(_interactionWithUI.IsInteracts);
    }

    private void OnDisable()
    {
        if (_interactionWithUI != null)
            _interactionWithUI.IsInteractsChanged -= SwitchCursorActive;
    }

    private void SwitchCursorActive(bool isActive)
    {
        Cursor.visible = isActive;
        Cursor.lockState = isActive ? CursorLockMode.None : CursorLockMode.Locked;
    }
}