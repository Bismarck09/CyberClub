using System;
using UnityEngine;

public class DevicePurchase : MonoBehaviour, IPurchasable
{
    [SerializeField] private CoinsData _coinsData;
    [SerializeField] private ZoneSwitcher _zoneSwitcher;

    private ZoneInformation _zoneInformation;
    private int _devicePrice;

    public event Action OnDevicePurchased;

    private void OnEnable()
    {
        _zoneSwitcher.OnZoneChanged += UpdateDevicePrice;
    }

    private void OnDisable()
    {
        _zoneSwitcher.OnZoneChanged -= UpdateDevicePrice;
    }

    public bool CanBuy()
    {
        return _coinsData.TryBuy(_devicePrice);
    }

    public void Buy()
    {
        if (CanBuy())
        {
            _zoneInformation.ZoneConfig.IncreaseDevicePrice();
            OnDevicePurchased?.Invoke();
        }
    }

    private void UpdateDevicePrice(ZoneInformation zoneInformation)
    {
        _zoneInformation = zoneInformation;
        _devicePrice = _zoneInformation.ZoneConfig.DevicePrice;
    }

}
