using UnityEngine;
using System.Collections.Generic;

public class GameDeviceFactory
{
    private List<IDeviceCreator> _deviceCreators;
    private DeviceRegistry _deviceRegistry;

    public GameDeviceFactory(List<IDeviceCreator> deviceCreators, DeviceRegistry deviceRegistry)
    {
        _deviceCreators = deviceCreators;
        _deviceRegistry = deviceRegistry;
    }

    public void SpawnDevice(ZoneDeviceConfig config, SpawnPointsHolder spawnPointsHolder)
    {
        IDeviceCreator creator = _deviceCreators.Find(c => c.Type == config.DeviceType);
        if (creator != null)
        {
            IGameDevice device = creator.Create(config, spawnPointsHolder);
            _deviceRegistry.Add(device, config.ZoneId.ToString());
        }
        else
        {
            Debug.LogError($"No device creator found for type {config.DeviceType}");
        }
    }
    
}
