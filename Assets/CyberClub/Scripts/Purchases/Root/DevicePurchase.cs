using System;
using UnityEngine;

public class DevicePurchase : MonoBehaviour, IPurchasable
{
    [SerializeField] private CoinsData _coinsData;
    [SerializeField] private ZoneSwitcher _zoneSwitcher;

    private ZoneInformation _zoneInformation;

    public event Action OnDevicePurchased;

    private void OnEnable()
    {
        _zoneSwitcher.OnZoneChanged += UpdateZoneInformation;
    }

    private void OnDisable()
    {
        _zoneSwitcher.OnZoneChanged -= UpdateZoneInformation;
    }

    public bool CanBuy()
    {
        if (_zoneInformation == null)
        {
            return false;
        }

        return _zoneInformation.SpawnPoints.HasSpawnPoints && _coinsData.TryBuy(_zoneInformation.ZoneConfig.DevicePrice) ;
    }

    public void Buy()
    {
        if (!CanBuy())
            return;

        _zoneInformation.ZoneConfig.IncreaseDevicePrice();
        OnDevicePurchased?.Invoke();
    }

    private void UpdateZoneInformation(ZoneInformation zoneInformation)
    {
        _zoneInformation = zoneInformation;
    }
}
