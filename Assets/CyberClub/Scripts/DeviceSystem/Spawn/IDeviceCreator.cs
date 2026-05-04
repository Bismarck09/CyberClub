using UnityEngine;

public interface IDeviceCreator
{
    DeviceType Type { get; }
    GameDevice Create(ZoneDeviceConfig config, Transform spawnPoint);
}
