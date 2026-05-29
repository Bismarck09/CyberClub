using System;
using UnityEngine;

public class InteriorPurchase : MonoBehaviour, IPurchasable
{
    [SerializeField] private ZoneSwitcher _zoneSwitcher;
    [SerializeField] private CoinsData _coinsData;
    
    private InteriorData _interiorData;
    
    public event Action<InteriorData> OnInteriorPurchase;

    private void OnEnable()
    {
        _zoneSwitcher.OnZoneChanged += ChangeInteriorData;
    }

    private void OnDisable()
    {
        _zoneSwitcher.OnZoneChanged -= ChangeInteriorData;
    }

    public bool CanBuy()
    {
        if  (_interiorData == null)
            return false;

        return !_interiorData.IsMaxPurchased && _coinsData.TryBuy(_interiorData.InteriorsPrice);
    }

    public void Buy()
    {
        if (CanBuy())
        {
            _interiorData.BuyInterior();
            OnInteriorPurchase?.Invoke(_interiorData);
        }
    }

    private void ChangeInteriorData(ZoneInformation zoneInformation)
    {
        _interiorData = zoneInformation.Interior;
    }
}
