using UnityEngine;

public class ZoneInformation : MonoBehaviour
{
    [SerializeField] private ZoneDeviceConfig _zoneDeviceConfig;
    [SerializeField] private SpawnPointsHolder _spawnPointsHolder;

    public ZoneDeviceConfig ZoneConfig => _zoneDeviceConfig;
    public SpawnPointsHolder SpawnPoints => _spawnPointsHolder;
}
