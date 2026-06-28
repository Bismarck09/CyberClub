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
        _visitorService.OnVisitorServiced += AddResources;
    }

    private void OnDisable()
    {
        _visitorService.OnVisitorServiced -= AddResources;
    }

    private void AddResources(DeviceEntry device)
    {
        if (device == null)
            return;

        float globalCoinsMultiplier = _resourceMultiplier.GetMultiplier(_coinsData.Type);
        float roomCoinsMultiplier = device.RoomCoinsMultiplier;
        float ratingMultiplier = _ratingData != null ? _ratingData.IncomeMultiplier : 1f;

        float coinsMultiplier = (globalCoinsMultiplier + roomCoinsMultiplier) * ratingMultiplier;

        _coinsData.AddResource(device.PriceOfHourCoins, coinsMultiplier);

        float gemsMultiplier = _resourceMultiplier.GetMultiplier(_gemsData.Type);
        _gemsData.AddResource(device.PriceOfHourGems, gemsMultiplier);
    }
}
