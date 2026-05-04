using UnityEngine;

public class ConsoleCreator : MonoBehaviour, IDeviceCreator
{
    public DeviceType Type => DeviceType.Console;

    public GameDevice Create(ZoneDeviceConfig config, Transform spawnPoint)
    {
        var consolePrefab = Instantiate(config.DevicePrefab, spawnPoint);

        if (consolePrefab.TryGetComponent<GameDevice>(out var gameDevice))
            return gameDevice;

        return consolePrefab.AddComponent<GameDevice>();
    }
}
