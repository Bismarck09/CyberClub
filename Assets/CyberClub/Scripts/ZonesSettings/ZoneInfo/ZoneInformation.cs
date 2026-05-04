using UnityEngine;

public class ZoneInformation : MonoBehaviour
{
    [SerializeField] private ZoneDeviceConfig _zoneDeviceConfig;
    [SerializeField] private SpawnPointsHolder _spawnPointsHolder;
    [SerializeField] private Color _color;
    [SerializeField] private string _zoneName;

    public ZoneDeviceConfig ZoneConfig => _zoneDeviceConfig;
    public SpawnPointsHolder SpawnPoints => _spawnPointsHolder;
    public Color ZoneColor => _color;
    public string ZoneName => _zoneName;
}
