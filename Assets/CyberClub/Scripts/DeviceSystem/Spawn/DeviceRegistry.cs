using System;
using System.Collections.Generic;
using UnityEngine;

public class DeviceRegistry : MonoBehaviour
{
    private readonly List<DeviceEntry> _devices = new();

    public int CurrentDeviceCount => _devices.Count;
    public IReadOnlyList<DeviceEntry> Devices => _devices;

    public event Action<DeviceEntry> OnDeviceAdded;

    public void Add(GameDevice device, ZoneInformation zoneInformation, int priceOfHourCoins, int priceOfHourGems)
    {
        TryAdd(device, zoneInformation, priceOfHourCoins, priceOfHourGems, out _);
    }

    public bool TryAdd(
        GameDevice device,
        ZoneInformation zoneInformation,
        int priceOfHourCoins,
        int priceOfHourGems,
        out DeviceEntry entry)
    {
        entry = null;

        if (device == null)
        {
            Debug.LogError("DeviceRegistry: попытка добавить null-устройство.");
            return false;
        }

        if (zoneInformation == null)
        {
            Debug.LogError("DeviceRegistry: устройство добавляется без ZoneInformation.");
            return false;
        }

        if (_devices.Exists(existingEntry =>
                existingEntry != null && existingEntry.Device == device))
        {
            Debug.LogError(
                $"DeviceRegistry: устройство {device.name} уже зарегистрировано.",
                device);

            return false;
        }

        entry = new DeviceEntry(device, zoneInformation, priceOfHourCoins, priceOfHourGems);
        _devices.Add(entry);

        try
        {
            OnDeviceAdded?.Invoke(entry);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
        }

        return true;
    }

    public DeviceEntry GetRandomFreeDevice()
    {
        List<DeviceEntry> freeDevices = _devices.FindAll(d => d.Device != null && d.Device.IsAvailable);

        if (freeDevices.Count <= 0)
            return null;

        int randomIndex = UnityEngine.Random.Range(0, freeDevices.Count);
        return freeDevices[randomIndex];
    }

    public List<DeviceEntry> GetBreakableDevices()
    {
        return _devices.FindAll(d => d.Device != null && d.Device.IsAvailable);
    }
}

public class DeviceEntry
{
    private readonly GameDevice _device;
    private readonly ZoneInformation _zoneInformation;
    private readonly int _priceOfHourCoins;
    private readonly int _legacyPriceOfHourGems;

    public GameDevice Device => _device;
    public ZoneInformation ZoneInformation => _zoneInformation;
    public int PriceOfHourCoins => _priceOfHourCoins;

    // Оставлено для совместимости со старым кодом.
    public int PriceOfHourGems => _legacyPriceOfHourGems;

    public float RoomCoinsMultiplier
    {
        get
        {
            if (_zoneInformation == null)
                return 0f;

            return _zoneInformation.GetCoinsMultiplier();
        }
    }

    public string ZoneName
    {
        get
        {
            if (_zoneInformation == null)
                return "Неизвестная комната";

            return string.IsNullOrWhiteSpace(_zoneInformation.ZoneName)
                ? _zoneInformation.name
                : _zoneInformation.ZoneName;
        }
    }

    public DeviceEntry(GameDevice device, ZoneInformation zoneInformation, int priceOfHourCoins, int priceOfHourGems)
    {
        _device = device;
        _zoneInformation = zoneInformation;
        _priceOfHourCoins = priceOfHourCoins;
        _legacyPriceOfHourGems = priceOfHourGems;
    }

    public int RollGemsReward()
    {
        if (_zoneInformation == null || _zoneInformation.ZoneConfig == null)
            return 0;

        return _zoneInformation.ZoneConfig.PriceOfHourGems;
    }
}
