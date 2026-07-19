using UnityEngine;
using System.Collections.Generic;

public class DeviceSpawner : MonoBehaviour
{
    [SerializeField] private DeviceRegistry _deviceRegistry;
    [SerializeField] private DevicePurchase _devicePurchase;

    private GameDeviceFactory _deviceFactory;

    private void Awake()
    {
        _deviceFactory = new GameDeviceFactory(
            new List<IDeviceCreator>
            {
                new ConsoleCreator(),
                new ComputerCreator()
            },
            _deviceRegistry
        );
    }

    private void OnEnable()
    {
        if (_devicePurchase != null)
            _devicePurchase.RegisterDeviceSpawner(this);
    }

    private void OnDisable()
    {
        if (_devicePurchase != null)
            _devicePurchase.UnregisterDeviceSpawner(this);
    }

    public void RestoreDevices(ZoneInformation zoneInformation, int count)
    {
        if (zoneInformation == null || zoneInformation.SpawnPoints == null)
            return;

        zoneInformation.SpawnPoints.ResetSpawnPoints();

        for (int i = 0; i < count; i++)
        {
            if (!TrySpawnDevice(zoneInformation, out _))
                break;
        }
    }

    public void SpawnDevice(ZoneInformation zoneInformation)
    {
        TrySpawnDevice(zoneInformation, out _);
    }

    public bool CanSpawnDevice(ZoneInformation zoneInformation)
    {
        if (_deviceFactory == null || _deviceRegistry == null || zoneInformation == null)
            return false;

        ZoneDeviceConfig config = zoneInformation.ZoneConfig;
        SpawnPointsHolder spawnPoints = zoneInformation.SpawnPoints;

        return config != null &&
               config.DevicePrefab != null &&
               spawnPoints != null &&
               spawnPoints.HasSpawnPoints &&
               _deviceFactory.CanCreateDevice(zoneInformation);
    }

    public bool TrySpawnDevice(ZoneInformation zoneInformation, out GameDevice device)
    {
        device = null;

        if (!CanSpawnDevice(zoneInformation))
            return false;

        return _deviceFactory.TrySpawnDevice(zoneInformation, out device);
    }
}
