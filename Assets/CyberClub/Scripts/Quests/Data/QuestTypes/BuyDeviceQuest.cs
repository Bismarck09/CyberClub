using UnityEngine;

public class BuyDeviceQuest : Quest
{
    private DevicePurchase _devicePurchase;

    public BuyDeviceQuest(DevicePurchase devicePurchase, IResource resource) : base(QuestType.BuyDevice, resource)
    {
        _devicePurchase = devicePurchase;
    }

    protected override void Subscribe()
    {
        _devicePurchase.OnDevicePurchased += Service;
    }

    protected override void Unsubscribe()
    {
        _devicePurchase.OnDevicePurchased -= Service;
    }

    private void Service()
    {
        AddProgress(1);
    }
}
