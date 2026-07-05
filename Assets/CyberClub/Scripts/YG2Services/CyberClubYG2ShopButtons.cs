using UnityEngine;

public class CyberClubYG2ShopButtons : MonoBehaviour
{
    [SerializeField] private CyberClubYG2RewardedAdsService _adsService;
    [SerializeField] private CyberClubYG2PaymentsService _paymentsService;

    public void WatchAdForCoins()
    {
        if (_adsService != null)
            _adsService.ShowCoinsRewardAd();
    }

    public void WatchAdForGems()
    {
        if (_adsService != null)
            _adsService.ShowGemsRewardAd();
    }

    public void BuyPremiumLocation()
    {
        if (_paymentsService != null)
            _paymentsService.BuyPremiumLocation();
    }
}