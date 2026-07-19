using UnityEngine;
using System;

[Serializable]
public class ZoneSaveSlot : MonoBehaviour
{
    [SerializeField] private string _id;
    [SerializeField] private ZoneInformation _zone;
    [SerializeField] private ZonePurchaseConfig _zonePurchaseConfig;
    [SerializeField] private DeviceSpawner _deviceSpawner;
    [SerializeField] private bool _isUnlockedByDefault;

    public string Id => _id;
    public ZoneInformation Zone => _zone;
    public DeviceSpawner DeviceSpawner => _deviceSpawner;
    public bool IsUnlockedByDefault => _isUnlockedByDefault;

    public bool IsUnlocked
    {
        get
        {
            if (_isUnlockedByDefault)
                return true;

            if (_zonePurchaseConfig == null)
                return true;

            return _zonePurchaseConfig.IsUnlocked;
        }
    }

    public void ApplyUnlockedState(bool isUnlocked)
    {
        if (_zonePurchaseConfig == null)
            return;

        _zonePurchaseConfig.RestoreUnlockedState(isUnlocked);
    }
}
