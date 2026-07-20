using System.Collections.Generic;
using UnityEngine;

public class ResourcesWallet : MonoBehaviour
{
    [SerializeField] private ResourcesMultiplier _resourceMultiplier;
    [SerializeField] private VisitorService _visitorService;
    [SerializeField] private CoinsData _coinsData;
    [SerializeField] private GemsData _gemsData;
    [SerializeField] private RatingData _ratingData;
    [SerializeField] private SaveLoadManager _saveLoadManager;

    private readonly List<IResource> _resources = new();

    private void Awake()
    {
        _resources.Add(_coinsData);
        _resources.Add(_gemsData);
    }

    private void OnEnable()
    {
        if (_visitorService != null)
            _visitorService.OnVisitorServiced += AddResources;
    }

    private void OnDisable()
    {
        if (_visitorService != null)
            _visitorService.OnVisitorServiced -= AddResources;
    }

    private void AddResources(DeviceEntry device)
    {
        if (device == null || _coinsData == null || _gemsData == null)
            return;

        ZoneDeviceConfig zoneConfig = device.ZoneInformation != null
            ? device.ZoneInformation.ZoneConfig
            : null;

        if (zoneConfig != null && zoneConfig.IsPremiumZone)
        {
            // Premium income is exact and ignores every multiplier.
            _coinsData.AddResource(zoneConfig.PriceOfHourCoins, 1f);

            if (zoneConfig.PriceOfHourGems > 0)
                _gemsData.AddResource(zoneConfig.PriceOfHourGems, 1f);

            _saveLoadManager?.SaveGame();
            return;
        }

        float globalCoinsMultiplier = _resourceMultiplier != null
            ? _resourceMultiplier.GetMultiplier(_coinsData.Type)
            : 1f;
        float roomCoinsMultiplier = device.RoomCoinsMultiplier;
        float ratingMultiplier = _ratingData != null ? _ratingData.IncomeMultiplier : 1f;
        float repairBonusMultiplier = device.Device != null
            ? device.Device.ConsumeRepairIncomeMultiplier()
            : 1f;

        float coinsMultiplier = (globalCoinsMultiplier + roomCoinsMultiplier) *
            ratingMultiplier * repairBonusMultiplier;
        _coinsData.AddResource(device.PriceOfHourCoins, coinsMultiplier);

        int gemsReward = device.RollGemsReward();

        if (gemsReward > 0)
            _gemsData.AddResource(gemsReward, 1f);

        _saveLoadManager?.SaveGame();
    }
}
