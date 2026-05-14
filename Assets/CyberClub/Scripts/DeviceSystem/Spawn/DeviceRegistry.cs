using UnityEngine;
using System.Collections.Generic;

public class DeviceRegistry : MonoBehaviour
{
    private List<DeviceEntry> _devices = new();

    public int CurrentDeviceCount => _devices.Count;

    public void Add(GameDevice device, int priceOfHourCoins, int priceOfHourGems)
    {
        _devices.Add(new DeviceEntry(device, priceOfHourCoins, priceOfHourGems));
    }

    public DeviceEntry GetRandomFreeDevice()
    {
        List<DeviceEntry> freeDevices = _devices.FindAll(d => !d.Device.IsOccupied);

        if (freeDevices.Count > 0)
        {
            int randomIndex = Random.Range(0, freeDevices.Count);
            return freeDevices[randomIndex];
        }

        return null;
    }
}

public class DeviceEntry
{
    private GameDevice _device;
    private  int _priceOfHourCoins;
    private int _priceOfHourGems;

    public GameDevice Device => _device;
    public int PriceOfHourCoins => _priceOfHourCoins;
    public int PriceOfHourGems => _priceOfHourGems;

    public DeviceEntry(GameDevice device, int priceOfHourCoins, int priceOfHourGems)
    {
        _device = device;
        _priceOfHourCoins = priceOfHourCoins;
        _priceOfHourGems = priceOfHourGems;
    }
}