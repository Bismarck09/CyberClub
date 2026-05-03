using UnityEngine;

public class CursorActive : MonoBehaviour
{
    [SerializeField] private InteractionWithUI _interactionWithUI;

    private void OnEnable() => _interactionWithUI.IsInteractsChanged += SwitchCursorActive;
    private void OnDisable() => _interactionWithUI.IsInteractsChanged -= SwitchCursorActive;

    private void SwitchCursorActive(bool isActive)
    {
        Cursor.visible = isActive;
        Cursor.lockState = isActive ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
