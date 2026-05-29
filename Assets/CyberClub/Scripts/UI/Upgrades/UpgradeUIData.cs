using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUIData : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _devicePriceText;
    [SerializeField] private ZoneSwitcher _zoneSwitcher;
    [SerializeField] private InteriorPurchase _interiorPurchase;
    [SerializeField] private DevicePurchase _devicePurchase;
    [SerializeField] private TextMeshProUGUI _interiorPrice;
    [SerializeField] private TextMeshProUGUI _interiorMultiplier;

    private ZoneInformation _zoneInformation;
    
    private void OnEnable()
    {
        _zoneSwitcher.OnZoneChanged += UpdateUpgradeUI;
        _interiorPurchase.OnInteriorPurchase += ChangeInteriorData;
        _devicePurchase.OnDevicePurchased += ChangeDeviceData;
        
    }
    
    private void OnDisable()
    {
        _zoneSwitcher.OnZoneChanged -= UpdateUpgradeUI;
        _interiorPurchase.OnInteriorPurchase -= ChangeInteriorData;
        _devicePurchase.OnDevicePurchased -= ChangeDeviceData;
    }

    private void UpdateUpgradeUI(ZoneInformation zoneInformation)
    {
        _zoneInformation = zoneInformation;
        
        ChangeDeviceData();
        ChangeInteriorData(zoneInformation.GetComponent<InteriorData>());
    }
    
    private void ChangeInteriorData(InteriorData interiorData)
    {
        if (interiorData.IsMaxPurchased)
            return;
        
        _interiorPrice.text = interiorData.InteriorsPrice.ToString();
        _interiorMultiplier.text = interiorData.GetCoinsMultiplier().ToString();
    }

    private void ChangeDeviceData()
    {
        _devicePriceText.text = _zoneInformation.ZoneConfig.DevicePrice.ToString();
    }
    
    
}
