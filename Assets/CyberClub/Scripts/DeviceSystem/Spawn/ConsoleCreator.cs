using UnityEngine;

public class ConsoleCreator : IDeviceCreator
{
    public DeviceType Type => DeviceType.Console;

    public GameDevice Create(ZoneDeviceConfig config, Transform spawnPoint)
    {
        if (config == null || config.DevicePrefab == null || spawnPoint == null)
        {
            Debug.LogError("ConsoleCreator: не переданы config, prefab или spawn point.");
            return null;
        }

        GameObject consolePrefab = Object.Instantiate(config.DevicePrefab, spawnPoint);

        if (consolePrefab.TryGetComponent(out GameDevice gameDevice))
            return gameDevice;

        Debug.LogError(
            $"ConsoleCreator: prefab {config.DevicePrefab.name} не содержит GameDevice. Созданный объект удалён.",
            config.DevicePrefab);

        consolePrefab.SetActive(false);
        Object.Destroy(consolePrefab);
        return null;
    }
}
