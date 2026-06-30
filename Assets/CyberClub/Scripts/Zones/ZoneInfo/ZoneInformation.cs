using UnityEngine;

public class ZoneInformation : MonoBehaviour
{
    [SerializeField] private ZoneDeviceConfig _zoneDeviceConfig;
    [SerializeField] private SpawnPointsHolder _spawnPointsHolder;
    [SerializeField] private Color _color;
    [SerializeField] private string _zoneName;
    [SerializeField] private InteriorData _interiorData;

    private ZoneRuntimeData _runtimeData;

    public ZoneDeviceConfig ZoneConfig => _zoneDeviceConfig;
    public SpawnPointsHolder SpawnPoints => _spawnPointsHolder;
    public InteriorData Interior => _interiorData;
    public Color ZoneColor => _color;
    public string ZoneName => _zoneName;

    public ZoneRuntimeData RuntimeData
    {
        get
        {
            if (_runtimeData == null)
                InitializeRuntimeData();

            return _runtimeData;
        }
    }

    public int CurrentDevicePrice => RuntimeData.CurrentDevicePrice;
    public int PurchasedDeviceCount => RuntimeData.PurchasedDeviceCount;

    private void Awake()
    {
        InitializeRuntimeData();
    }

    public float GetCoinsMultiplier()
    {
        return _interiorData != null ? _interiorData.GetCoinsMultiplier() : 0f;
    }

    public void RegisterDevicePurchase()
    {
        RuntimeData.RegisterDevicePurchase();
    }

    public void ResetRuntimeData()
    {
        InitializeRuntimeData(0);
    }

    public ZoneRuntimeSaveData GetRuntimeSaveData()
    {
        return RuntimeData.ToSaveData(ZoneName);
    }

    public void ApplyRuntimeSaveData(ZoneRuntimeSaveData saveData)
    {
        RuntimeData.ApplySaveData(saveData);
    }

    private void InitializeRuntimeData(int purchasedDeviceCount = 0)
    {
        _runtimeData ??= new ZoneRuntimeData();
        _runtimeData.Initialize(_zoneDeviceConfig, purchasedDeviceCount);
    }
}