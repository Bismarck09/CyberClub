using UnityEngine;

public class ComputerCreator : IDeviceCreator
{
    public DeviceType Type => DeviceType.Computer;

    public GameDevice Create(ZoneDeviceConfig config, Transform spawnPoint)
    {
        if (config == null || config.DevicePrefab == null || spawnPoint == null)
        {
            Debug.LogError("ComputerCreator: не переданы config, prefab или spawn point.");
            return null;
        }

        GameObject computerPrefab = Object.Instantiate(config.DevicePrefab, spawnPoint);

        if (computerPrefab.TryGetComponent(out GameDevice gameDevice))
            return gameDevice;

        Debug.LogError(
            $"ComputerCreator: prefab {config.DevicePrefab.name} не содержит GameDevice. Созданный объект удалён.",
            config.DevicePrefab);

        computerPrefab.SetActive(false);
        Object.Destroy(computerPrefab);
        return null;
    }
}
