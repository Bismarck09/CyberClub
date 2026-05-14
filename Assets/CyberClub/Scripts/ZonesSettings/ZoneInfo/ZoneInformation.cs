using UnityEngine;
using System.Collections.Generic;

public class ZoneInformation : MonoBehaviour
{
    [SerializeField] private ZoneDeviceConfig _zoneDeviceConfig;
    [SerializeField] private SpawnPointsHolder _spawnPointsHolder;
    [SerializeField] private Color _color;
    [SerializeField] private string _zoneName;
    [SerializeField] private InteriorData _interiorData;

    public ZoneDeviceConfig ZoneConfig => _zoneDeviceConfig;
    public SpawnPointsHolder SpawnPoints => _spawnPointsHolder;
    public Color ZoneColor => _color;
    public string ZoneName => _zoneName;

    public float GetCoinsMultiplier()
    {
        return _interiorData.GetCoinsMultiplier();
    }
}
