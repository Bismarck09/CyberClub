using UnityEngine;
using System.Collections.Generic;

public class DeviceSpawner : MonoBehaviour
{
    [SerializeField] private DeviceRegistry deviceRegistry;

    private GameDeviceFactory _deviceFactory;

    private void Awake()
    {
        _deviceFactory = new GameDeviceFactory(new List<IDeviceCreator>() { new ConsoleCreator(), new ComputerCreator() }, deviceRegistry);
    }

    public void SpawnDevice(ZoneDeviceConfig config, SpawnPointsHolder spawnPointsHolder)
    {
        _deviceFactory.SpawnDevice(config, spawnPointsHolder);
    }
}
