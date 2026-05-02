using UnityEngine;
using System.Collections.Generic;

public class DeviceSpawner : MonoBehaviour
{
    [SerializeField] private DeviceRegistry _deviceRegistry;
    [SerializeField] private LocationInformation _locationInformation;

    private GameDeviceFactory _deviceFactory;

    private void Awake()
    {
        _deviceFactory = new GameDeviceFactory(new List<IDeviceCreator>() { new ConsoleCreator(), new ComputerCreator() }, _deviceRegistry);
    }

    public void SpawnDevice()
    {
        if (_locationInformation.IsDeviceRemained)
            _deviceFactory.SpawnDevice(_locationInformation.ZoneConfig, _locationInformation.SpawnPoints);
    }

}
