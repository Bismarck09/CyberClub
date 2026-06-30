using System;
using UnityEngine;

[Serializable]
public class ZoneRuntimeData
{
    [SerializeField] private int _purchasedDeviceCount;

    private ZoneDeviceConfig _config;

    public int PurchasedDeviceCount => _purchasedDeviceCount;

    public int CurrentDevicePrice
    {
        get
        {
            if (_config == null)
                return 0;

            return _config.CalculateDevicePrice(_purchasedDeviceCount);
        }
    }

    public event Action OnChanged;

    public void Initialize(ZoneDeviceConfig config, int purchasedDeviceCount = 0)
    {
        _config = config;
        _purchasedDeviceCount = Mathf.Max(0, purchasedDeviceCount);
        OnChanged?.Invoke();
    }

    public void RegisterDevicePurchase()
    {
        _purchasedDeviceCount++;
        OnChanged?.Invoke();
    }

    public void SetPurchasedDeviceCount(int value)
    {
        _purchasedDeviceCount = Mathf.Max(0, value);
        OnChanged?.Invoke();
    }

    public void ResetProgress()
    {
        _purchasedDeviceCount = 0;
        OnChanged?.Invoke();
    }

    public ZoneRuntimeSaveData ToSaveData(string zoneId)
    {
        return new ZoneRuntimeSaveData(zoneId, _purchasedDeviceCount);
    }

    public void ApplySaveData(ZoneRuntimeSaveData saveData)
    {
        if (saveData == null)
            return;

        SetPurchasedDeviceCount(saveData.PurchasedDeviceCount);
    }
}