using UnityEngine;

public class ZoneInformation : MonoBehaviour
{
    [SerializeField] private ZoneDeviceConfig _zoneDeviceConfig;
    [SerializeField] private SpawnPointsHolder _spawnPointsHolder;
    [SerializeField] private Color _color;
    [SerializeField] private string _zoneName;
    [SerializeField] private InteriorData _interiorData;

    private int _currentDevicePurchases;

    public ZoneDeviceConfig ZoneConfig => _zoneDeviceConfig;
    public SpawnPointsHolder SpawnPoints => _spawnPointsHolder;
    public InteriorData Interior => _interiorData;
    public Color ZoneColor => _color;
    public string ZoneName => _zoneName;

    public int CurrentDevicePurchases => _currentDevicePurchases;
    public int CurrentDevicePrice => _zoneDeviceConfig != null ? _zoneDeviceConfig.CalculateDevicePrice(_currentDevicePurchases) : 0;

    public void RegisterDevicePurchase()
    {
        _currentDevicePurchases++;
    }

    public void RestoreDevicePurchases(int count)
    {
        _currentDevicePurchases = Mathf.Max(0, count);
    }

    public float GetCoinsMultiplier()
    {
        return _interiorData != null ? _interiorData.GetCoinsMultiplier() : 0f;
    }
}