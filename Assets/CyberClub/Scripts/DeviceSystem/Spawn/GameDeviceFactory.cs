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
        TrySpawnDevice(zoneInformation, out _);
    }

    public bool CanCreateDevice(ZoneInformation zoneInformation)
    {
        if (zoneInformation == null || _deviceRegistry == null)
            return false;

        ZoneDeviceConfig config = zoneInformation.ZoneConfig;
        SpawnPointsHolder spawnPointsHolder = zoneInformation.SpawnPoints;

        if (config == null || config.DevicePrefab == null || spawnPointsHolder == null)
            return false;

        return spawnPointsHolder.HasSpawnPoints &&
               _deviceCreators.Exists(creator => creator != null && creator.Type == config.DeviceType);
    }

    public bool TrySpawnDevice(ZoneInformation zoneInformation, out GameDevice device)
    {
        device = null;

        if (zoneInformation == null)
        {
            Debug.LogError("GameDeviceFactory: не передана ZoneInformation.");
            return false;
        }

        if (_deviceRegistry == null)
        {
            Debug.LogError("GameDeviceFactory: не назначен DeviceRegistry.");
            return false;
        }

        ZoneDeviceConfig config = zoneInformation.ZoneConfig;
        SpawnPointsHolder spawnPointsHolder = zoneInformation.SpawnPoints;

        if (config == null)
        {
            Debug.LogError($"GameDeviceFactory: у зоны {zoneInformation.ZoneName} не назначен ZoneDeviceConfig.");
            return false;
        }

        if (config.DevicePrefab == null)
        {
            Debug.LogError($"GameDeviceFactory: у конфигурации {config.name} не назначен prefab устройства.");
            return false;
        }

        if (spawnPointsHolder == null)
        {
            Debug.LogError($"GameDeviceFactory: у зоны {zoneInformation.ZoneName} не назначен SpawnPointsHolder.");
            return false;
        }

        IDeviceCreator creator = _deviceCreators.Find(c => c != null && c.Type == config.DeviceType);

        if (creator == null)
        {
            Debug.LogError($"GameDeviceFactory: не найден creator для типа {config.DeviceType}.");
            return false;
        }

        Transform spawnPoint = spawnPointsHolder.GetSpawnPoint();

        if (spawnPoint == null)
        {
            IsDeviceOver?.Invoke();
            return false;
        }

        GameDevice createdDevice = null;

        try
        {
            createdDevice = creator.Create(config, spawnPoint);

            if (createdDevice == null)
                return false;

            if (createdDevice.TargetPoint == null)
            {
                Debug.LogError(
                    $"GameDeviceFactory: у устройства {createdDevice.name} не назначен TargetPoint.",
                    createdDevice);

                return false;
            }

            if (!_deviceRegistry.TryAdd(
                    createdDevice,
                    zoneInformation,
                    config.PriceOfHourCoins,
                    config.PriceOfHourGems,
                    out _))
            {
                return false;
            }

            device = createdDevice;
            return true;
        }
        finally
        {
            if (device == null)
            {
                spawnPointsHolder.ReleaseSpawnPoint(spawnPoint);

                if (createdDevice != null)
                {
                    createdDevice.gameObject.SetActive(false);
                    UnityEngine.Object.Destroy(createdDevice.gameObject);
                }
            }
        }
    }
}
