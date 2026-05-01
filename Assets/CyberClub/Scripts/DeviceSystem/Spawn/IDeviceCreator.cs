using UnityEngine;

public interface IDeviceCreator
{
    DeviceType Type { get; }
    IGameDevice Create(ZoneDeviceConfig config, SpawnPointsHolder spawnPointsHolder);
}
