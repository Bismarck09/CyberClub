using System;

[Serializable]
public class ZoneRuntimeSaveData
{
    public string ZoneId;
    public int PurchasedDeviceCount;

    public ZoneRuntimeSaveData(string zoneId, int purchasedDeviceCount)
    {
        ZoneId = zoneId;
        PurchasedDeviceCount = purchasedDeviceCount;
    }
}