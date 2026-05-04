using UnityEngine;
using System.Collections.Generic;

public class DeviceRegistry : MonoBehaviour
{
    private List<DeviceEntry> _devices = new();

    public void Add(GameDevice device, float priceOfHourCoins, float priceOfHourGems)
    {
        _devices.Add(new DeviceEntry(device, priceOfHourCoins, priceOfHourGems));
    }
}

public class DeviceEntry
{
    private GameDevice _device;
    private  float _priceOfHourCoins;
    private float _priceOfHourGems;

    public GameDevice Device => _device;
    public float PriceOfHourCoins => _priceOfHourCoins;
    public float PriceOfHourGems => _priceOfHourGems;

    public DeviceEntry(GameDevice device, float priceOfHourCoins, float priceOfHourGems)
    {
        _device = device;
        _priceOfHourCoins = priceOfHourCoins;
        _priceOfHourGems = priceOfHourGems;
    }
}