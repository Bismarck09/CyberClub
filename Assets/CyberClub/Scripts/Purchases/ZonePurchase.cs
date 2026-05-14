using UnityEngine;
using System;
using UnityEngine.Windows.Speech;

public class ZonePurchase : MonoBehaviour, IPurchasable
{
    [SerializeField] private CoinsData _coinsData;
    
    private ZonePurchaseConfig _zonePurchaseConfig;
    private BarrierDissolve _barrierData;

    public event Action OnZonePurchased;

    public void Buy()
    {
        if (CanBuy())
        {
            OnZonePurchased?.Invoke();
        }
    }


    public bool CanBuy()
    {
        return _coinsData.TryBuy(_zonePurchaseConfig.ZonePrice);
    }

    public void ChangeZoneData(ZonePurchaseConfig config)
    {
        _zonePurchaseConfig = config;
        _barrierData = config.GetComponent<BarrierDissolve>();
        _barrierData.Init();
    }
}
