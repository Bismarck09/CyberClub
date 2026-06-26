using UnityEngine;
using System.Collections.Generic;

public class DeviceRegistry : MonoBehaviour
{
    private readonly List<DeviceEntry> _devices = new();

    public int CurrentDeviceCount => _devices.Count;

    public void Add(GameDevice device, ZoneInformation zoneInformation, int priceOfHourCoins, int priceOfHourGems)
    {
        if (device == null)
        {
            Debug.LogError("DeviceRegistry: попытка добавить null-устройство.");
            return;
        }

        if (zoneInformation == null)
        {
            Debug.LogError("DeviceRegistry: устройство добавляется без ZoneInformation. Множитель комнаты будет считаться неправильно.");
            return;
        }

        _devices.Add(new DeviceEntry(device, zoneInformation, priceOfHourCoins, priceOfHourGems));
    }

    public DeviceEntry GetRandomFreeDevice()
    {
        List<DeviceEntry> freeDevices = _devices.FindAll(d => d.Device != null && !d.Device.IsOccupied);

        if (freeDevices.Count <= 0)
            return null;

        int randomIndex = Random.Range(0, freeDevices.Count);
        return freeDevices[randomIndex];
    }
}

public class DeviceEntry
{
    private readonly GameDevice _device;
    private readonly ZoneInformation _zoneInformation;
    private readonly int _priceOfHourCoins;
    private readonly int _priceOfHourGems;

    public GameDevice Device => _device;
    public ZoneInformation ZoneInformation => _zoneInformation;
    public int PriceOfHourCoins => _priceOfHourCoins;
    public int PriceOfHourGems => _priceOfHourGems;

    public float RoomCoinsMultiplier
    {
        get
        {
            if (_zoneInformation == null)
                return 0f;

            return _zoneInformation.GetCoinsMultiplier();
        }
    }

    public DeviceEntry(GameDevice device, ZoneInformation zoneInformation, int priceOfHourCoins, int priceOfHourGems)
    {
        _device = device;
        _zoneInformation = zoneInformation;
        _priceOfHourCoins = priceOfHourCoins;
        _priceOfHourGems = priceOfHourGems;
    }
}