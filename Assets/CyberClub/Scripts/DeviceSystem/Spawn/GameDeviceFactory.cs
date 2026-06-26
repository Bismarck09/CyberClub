using UnityEngine;
using System;
using System.Collections.Generic;

public class GameDeviceFactory
{
    private readonly List<IDeviceCreator> _deviceCreators;
    private readonly DeviceRegistry _deviceRegistry;

    public event Action IsDeviceOver;

    public GameDeviceFactory(List<IDeviceCreator> deviceCreators, DeviceRegistry deviceRegistry)
    {
        _deviceCreators = deviceCreators;
        _deviceRegistry = deviceRegistry;
    }

    public void SpawnDevice(ZoneInformation zoneInformation)
    {
        if (zoneInformation == null)
        {
            Debug.LogError("GameDeviceFactory: не передана ZoneInformation.");
            return;
        }

        ZoneDeviceConfig config = zoneInformation.ZoneConfig;
        SpawnPointsHolder spawnPointsHolder = zoneInformation.SpawnPoints;

        if (config == null)
        {
            Debug.LogError($"GameDeviceFactory: у зоны {zoneInformation.ZoneName} не назначен ZoneDeviceConfig.");
            return;
        }

        if (spawnPointsHolder == null)
        {
            Debug.LogError($"GameDeviceFactory: у зоны {zoneInformation.ZoneName} не назначен SpawnPointsHolder.");
            return;
        }

        IDeviceCreator creator = _deviceCreators.Find(c => c.Type == config.DeviceType);

        if (creator == null)
        {
            Debug.LogError($"GameDeviceFactory: не найден creator для типа {config.DeviceType}.");
            return;
        }

        Transform spawnPoint = spawnPointsHolder.GetSpawnPoint();

        if (spawnPoint == null)
        {
            IsDeviceOver?.Invoke();
            return;
        }

        GameDevice device = creator.Create(config, spawnPoint);

        _deviceRegistry.Add(
            device,
            zoneInformation,
            config.PriceOfHourCoins,
            config.PriceOfHourGems
        );
    }
}