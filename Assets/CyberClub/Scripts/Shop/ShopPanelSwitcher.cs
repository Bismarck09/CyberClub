using UnityEngine;
using UnityEngine.UI;

public class ShopPanelSwitcher : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _potionsButton;
    [SerializeField] private Button _resourcesButton;

    [Header("Panels")]
    [SerializeField] private GameObject _potionsPanel;
    [SerializeField] private GameObject _resourcesPanel;

    private void OnEnable()
    {
        if (_potionsButton != null)
            _potionsButton.onClick.AddListener(ShowPotions);

        if (_resourcesButton != null)
            _resourcesButton.onClick.AddListener(ShowResources);

        ShowPotions();
    }

    private void OnDisable()
    {
        if (_potionsButton != null)
            _potionsButton.onClick.RemoveListener(ShowPotions);

        if (_resourcesButton != null)
            _resourcesButton.onClick.RemoveListener(ShowResources);
    }

    public void ShowPotions()
    {
        SetPanelState(_potionsPanel, true);
        SetPanelState(_resourcesPanel, false);
    }

    public void ShowResources()
    {
        SetPanelState(_potionsPanel, false);
        SetPanelState(_resourcesPanel, true);
    }

    private void SetPanelState(GameObject panel, bool state)
    {
        if (panel != null)
            panel.SetActive(state);
    }
}