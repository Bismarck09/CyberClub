using System;
using UnityEngine;

public class DevicePurchase : MonoBehaviour, IPurchasable
{
    [SerializeField] private CoinsData _coinsData;
    [SerializeField] private ZoneSwitcher _zoneSwitcher;

    private ZoneInformation _zoneInformation;

    public event Action OnDevicePurchased;
    public event Action OnDeviceStateChanged;
    public event Action<int> OnDevicePriceChanged;

    public int CurrentDevicePrice => _zoneInformation != null ? _zoneInformation.CurrentDevicePrice : 0;

    public bool IsDeviceLimitReached
    {
        get
        {
            if (_zoneInformation == null)
                return true;

            if (_zoneInformation.SpawnPoints == null)
                return true;

            return !_zoneInformation.SpawnPoints.HasSpawnPoints;
        }
    }

    private void OnEnable()
    {
        if (_zoneSwitcher != null)
            _zoneSwitcher.OnZoneChanged += UpdateZoneInformation;
    }

    private void OnDisable()
    {
        if (_zoneSwitcher != null)
            _zoneSwitcher.OnZoneChanged -= UpdateZoneInformation;
    }

    public bool CanBuy()
    {
        if (_zoneInformation == null || _coinsData == null)
            return false;

        if (IsDeviceLimitReached)
            return false;

        return _coinsData.CurrentCoins >= CurrentDevicePrice;
    }

    public void Buy()
    {
        if (_zoneInformation == null || _coinsData == null)
        {
            NotifyDeviceStateChanged();
            return;
        }

        if (IsDeviceLimitReached)
        {
            NotifyDeviceStateChanged();
            return;
        }

        int price = CurrentDevicePrice;

        if (!_coinsData.TryBuy(price))
        {
            NotifyDeviceStateChanged();
            return;
        }

        _zoneInformation.RegisterDevicePurchase();
        OnDevicePurchased?.Invoke();
        NotifyDeviceStateChanged();
    }

    private void UpdateZoneInformation(ZoneInformation zoneInformation)
    {
        _zoneInformation = zoneInformation;
        NotifyDeviceStateChanged();
    }

    private void NotifyDeviceStateChanged()
    {
        OnDevicePriceChanged?.Invoke(CurrentDevicePrice);
        OnDeviceStateChanged?.Invoke();
    }
}
