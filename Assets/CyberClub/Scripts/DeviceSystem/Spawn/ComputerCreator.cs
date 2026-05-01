using UnityEngine;

public class ComputerCreator : MonoBehaviour, IDeviceCreator
{
    public DeviceType Type => DeviceType.Computer;

    public IGameDevice Create(ZoneDeviceConfig config, SpawnPointsHolder spawnPointsHolder)
    {
        var computerPrefab = Instantiate(config.DevicePrefab, spawnPointsHolder.GetSpawnPoint());
        return computerPrefab.GetComponent<IGameDevice>();
    }
}
