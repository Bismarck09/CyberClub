using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUIData : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _devicePriceText;
    [SerializeField] private ZoneSwitcher _zoneSwitcher;

    private void OnEnable()
    {
        _zoneSwitcher.OnZoneChanged += UpdateDevicePrice;
    }
    
    private void OnDisable()
    {
        _zoneSwitcher.OnZoneChanged -= UpdateDevicePrice;
    }

    private void UpdateDevicePrice(ZoneInformation zoneInformation)
    {
        _devicePriceText.text = zoneInformation.ZoneConfig.DevicePrice.ToString();
    }
}
