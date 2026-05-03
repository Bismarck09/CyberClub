using UnityEngine;

public class ConsoleCreator : MonoBehaviour, IDeviceCreator
{
    public DeviceType Type => DeviceType.Console;

    public IGameDevice Create(ZoneDeviceConfig config, Transform spawnPoint)
    {
        var consolePrefab = Instantiate(config.DevicePrefab, spawnPoint);
        return consolePrefab.GetComponent<IGameDevice>();
    }
}
