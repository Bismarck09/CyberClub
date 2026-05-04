using UnityEngine;
using UnityEngine.UI;

public class UpgradeUIActive : MonoBehaviour
{
    [SerializeField] private Button _upgradeButton;
    [SerializeField] private GameObject _upgradePanel;
    [SerializeField] private ZoneSwitcher _zoneSwitcher;

    private void OnEnable()
    {
        _zoneSwitcher.OnZoneChanged += EnableUpgrades;
        _zoneSwitcher.OnZoneExited += DisableUpgrades;
    }

    private void OnDisable()
    {
        _zoneSwitcher.OnZoneChanged -= EnableUpgrades;
        _zoneSwitcher.OnZoneExited -= DisableUpgrades;
    }

    private void DisableUpgrades()
    {
        _upgradeButton.interactable = false;
        _upgradePanel.SetActive(false);
    }

    private void EnableUpgrades(ZoneInformation newZoneInformation)
    {
        _upgradeButton.interactable = true;
    }

}
