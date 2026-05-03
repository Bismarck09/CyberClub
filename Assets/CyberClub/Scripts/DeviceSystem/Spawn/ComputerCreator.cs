using UnityEngine;

public class ComputerCreator : MonoBehaviour, IDeviceCreator
{
    public DeviceType Type => DeviceType.Computer;

    public IGameDevice Create(ZoneDeviceConfig config, Transform spawnPoint)
    {
        var computerPrefab = Instantiate(config.DevicePrefab, spawnPoint);
        return computerPrefab.GetComponent<IGameDevice>();
    }
}
