using UnityEngine;

public class ConsoleCreator : MonoBehaviour, IDeviceCreator
{
    public DeviceType Type => DeviceType.Console;

    public IGameDevice Create(ZoneDeviceConfig config, SpawnPointsHolder spawnPointsHolder)
    {
        var consolePrefab = Instantiate(config.DevicePrefab, spawnPointsHolder.GetSpawnPoint());
        return consolePrefab.GetComponent<IGameDevice>();
    }
}
