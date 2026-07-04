using UnityEngine;
using System.Collections.Generic;

public class DeviceSpawner : MonoBehaviour
{
    [SerializeField] private DeviceRegistry _deviceRegistry;
    [SerializeField] private LocationInformation _locationInformation;
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
            _devicePurchase.OnDevicePurchased += SpawnDeviceInCurrentZone;
    }

    private void OnDisable()
    {
        if (_devicePurchase != null)
            _devicePurchase.OnDevicePurchased -= SpawnDeviceInCurrentZone;
    }

    private void SpawnDeviceInCurrentZone()
    {
        if (_locationInformation == null || _locationInformation.CurrentZoneInformation == null)
        {
            Debug.LogError("DeviceSpawner: нет текущей зоны для спавна устройства.");
            return;
        }

        SpawnDevice(_locationInformation.CurrentZoneInformation);
    }

    public void RestoreDevices(ZoneInformation zoneInformation, int count)
    {
        if (zoneInformation == null || zoneInformation.SpawnPoints == null)
            return;

        zoneInformation.SpawnPoints.ResetSpawnPoints();

        for (int i = 0; i < count; i++)
            SpawnDevice(zoneInformation);
    }

    public void SpawnDevice(ZoneInformation zoneInformation)
    {
        if (zoneInformation == null)
            return;

        if (zoneInformation.SpawnPoints == null || zoneInformation.SpawnPoints.HasSpawnPoints == false)
            return;

        _deviceFactory.SpawnDevice(zoneInformation);
    }
}