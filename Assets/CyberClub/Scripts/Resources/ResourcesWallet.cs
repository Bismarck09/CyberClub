using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ResourcesWallet : MonoBehaviour
{
    [SerializeField] private ResourcesMultiplier _resourceMultiplier;
    [SerializeField] private VisitorService _visitorService;
    [SerializeField] private CoinsData _coinsData;
    [SerializeField] private GemsData _gemsData;

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
        _coinsData.AddResource(device.PriceOfHourCoins, _resourceMultiplier.GetMultiplier(_coinsData.Type));
        _gemsData.AddResource(device.PriceOfHourGems, _resourceMultiplier.GetMultiplier(_gemsData.Type));
    }

    private void RemoveResource()
    {
        
    }
}
