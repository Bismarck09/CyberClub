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
        if (_zoneSwitcher != null)
            _zoneSwitcher.OnZoneChanged += ChangeInteriorData;
    }

    private void OnDisable()
    {
        if (_zoneSwitcher != null)
            _zoneSwitcher.OnZoneChanged -= ChangeInteriorData;
    }

    public bool CanBuy()
    {
        if (_interiorData == null || _coinsData == null)
            return false;

        if (_interiorData.IsMaxPurchased)
            return false;

        return _coinsData.CurrentCoins >= _interiorData.InteriorsPrice;
    }

    public void Buy()
    {
        if (!CanBuy())
            return;

        if (!_coinsData.TryBuy(_interiorData.InteriorsPrice))
            return;

        _interiorData.BuyInterior();
        OnInteriorPurchase?.Invoke(_interiorData);
    }

    private void ChangeInteriorData(ZoneInformation zoneInformation)
    {
        _interiorData = zoneInformation != null ? zoneInformation.Interior : null;
    }
}