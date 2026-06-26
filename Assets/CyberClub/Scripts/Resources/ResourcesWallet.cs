using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ResourcesWallet : MonoBehaviour
{
    [SerializeField] private ResourcesMultiplier _resourceMultiplier;
    [SerializeField] private VisitorService _visitorService;
    [SerializeField] private CoinsData _coinsData;
    [SerializeField] private GemsData _gemsData;
    [SerializeField] private LocationInformation _locationInformation;

    private List<IResource> _resources = new();

    void Awake()
    {
        _resources.Add(_coinsData);
        _resources.Add(_gemsData);
    }

    void OnEnable()
    {
        _visitorService.OnVisitorServiced += AddResources;
    }

    void OnDisable()
    {
        _visitorService.OnVisitorServiced -= AddResources;
    }

    private void AddResources(DeviceEntry device)
    {
        if (device == null)
            return;

        float globalCoinsMultiplier = _resourceMultiplier.GetMultiplier(_coinsData.Type);
        float roomCoinsMultiplier = device.RoomCoinsMultiplier;

        float coinsMultiplier = globalCoinsMultiplier + roomCoinsMultiplier;

        _coinsData.AddResource(device.PriceOfHourCoins, coinsMultiplier);

        float gemsMultiplier = _resourceMultiplier.GetMultiplier(_gemsData.Type);
        _gemsData.AddResource(device.PriceOfHourGems, gemsMultiplier);
    }
}
