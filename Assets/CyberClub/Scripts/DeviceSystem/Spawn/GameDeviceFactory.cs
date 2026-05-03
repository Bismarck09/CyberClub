using UnityEngine;
using System.Collections.Generic;
using System;

public class GameDeviceFactory
{
    private List<IDeviceCreator> _deviceCreators;
    private DeviceRegistry _deviceRegistry;

    public event Action IsDeviceOver;

    public GameDeviceFactory(List<IDeviceCreator> deviceCreators, DeviceRegistry deviceRegistry)
    {
        _deviceCreators = deviceCreators;
        _deviceRegistry = deviceRegistry;
    }

    public void SpawnDevice(ZoneDeviceConfig config, SpawnPointsHolder spawnPointsHolder)
    {
        IDeviceCreator creator = _deviceCreators.Find(c => c.Type == config.DeviceType);
        Transform spawnPoint = spawnPointsHolder.GetSpawnPoint();

        if (spawnPoint != null)
        {
            IGameDevice device = creator.Create(config, spawnPoint);
            _deviceRegistry.Add(device, config.ZoneId.ToString());
        }
        else
        {
            IsDeviceOver?.Invoke();
        }
    }
    
}
