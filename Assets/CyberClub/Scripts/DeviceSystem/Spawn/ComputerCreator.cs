using UnityEngine;

public class ComputerCreator : MonoBehaviour, IDeviceCreator
{
    public DeviceType Type => DeviceType.Computer;

    public GameDevice Create(ZoneDeviceConfig config, Transform spawnPoint)
    {
        var computerPrefab = Instantiate(config.DevicePrefab, spawnPoint);

        if (computerPrefab.TryGetComponent<GameDevice>(out var gameDevice))
            return gameDevice;

        return computerPrefab.AddComponent<GameDevice>();
    }
}
