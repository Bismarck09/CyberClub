using UnityEngine;
using System.Collections.Generic;

public class DeviceRegistry : MonoBehaviour
{
    private List<DeviceEntry> _devices = new();

    public void Add(IGameDevice device, string zoneId)
    {
        _devices.Add(new DeviceEntry(device, zoneId));
    }
}

public class DeviceEntry
{
    public IGameDevice Device;
    public string ZoneId;

    public DeviceEntry(IGameDevice device, string zoneId)
    {
        Device = device;
        ZoneId = zoneId;
    }
}