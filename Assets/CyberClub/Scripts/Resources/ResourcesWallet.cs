using System.Collections.Generic;
using UnityEngine;

public class ResourcesWallet : MonoBehaviour
{
    [SerializeField] private ResourcesMultiplier _resourceMultiplier;
    [SerializeField] private VisitorService _visitorService;
    [SerializeField] private CoinsData _coinsData;
    [SerializeField] private GemsData _gemsData;
    [SerializeField] private RatingData _ratingData;

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
        if (device == null)
            return;

        float globalCoinsMultiplier = _resourceMultiplier != null ? _resourceMultiplier.GetMultiplier(_coinsData.Type) : 1f;
        float roomCoinsMultiplier = device.RoomCoinsMultiplier;
        float ratingMultiplier = _ratingData != null ? _ratingData.IncomeMultiplier : 1f;
        float repairBonusMultiplier = device.Device != null ? device.Device.ConsumeRepairIncomeMultiplier() : 1f;

        float coinsMultiplier = (globalCoinsMultiplier + roomCoinsMultiplier) * ratingMultiplier * repairBonusMultiplier;
        _coinsData.AddResource(device.PriceOfHourCoins, coinsMultiplier);

        int gemsReward = device.RollGemsReward();

        if (gemsReward <= 0)
            return;

        float gemsMultiplier = _resourceMultiplier != null ? _resourceMultiplier.GetMultiplier(_gemsData.Type) : 1f;
        _gemsData.AddResource(gemsReward, gemsMultiplier);
    }
}