using UnityEngine;
using YG;

public class ShopActionService : MonoBehaviour
{
    [Header("Resources")]
    [SerializeField] private GemsData _gemsData;
    [SerializeField] private CoinsData _coinsData;

    [Header("Potion effects")]
    [SerializeField] private PotionEffectService _potionEffectService;

    [Header("Save")]
    [SerializeField] private SaveLoadManager _saveLoadManager;

    [Header("Rewarded ad IDs")]
    [SerializeField] private string _gemsRewardAdId = "reward_gems";
    [SerializeField] private string _coinsRewardAdId = "reward_coins";

    private bool _isRewardAdOpening;

    public void Execute(ShopProductConfig product)
    {
        if (product == null)
        {
            Debug.LogWarning("ShopActionService: product is null.");
            return;
        }

        switch (product.ActionType)
        {
            case ShopProductActionType.Potion:
                BuyPotion(product);
                break;

            case ShopProductActionType.RewardGems:
                ShowRewardAd(_gemsRewardAdId, () =>
                {
                    AddGems(product.RewardAmount);
                });
                break;

            case ShopProductActionType.RewardCoins:
                ShowRewardAd(_coinsRewardAdId, () =>
                {
                    AddCoins(product.RewardAmount);
                });
                break;

            case ShopProductActionType.Unavailable:
                Debug.Log($"Товар {product.name} пока недоступен.");
                break;

            default:
                Debug.LogWarning($"ShopActionService: неизвестный тип действия {product.ActionType}.");
                break;
        }
    }

    private void ShowRewardAd(string rewardId, System.Action onReward)
    {
        if (_isRewardAdOpening)
            return;

        _isRewardAdOpening = true;

        YG2.RewardedAdvShow(rewardId, () =>
        {
            _isRewardAdOpening = false;

            onReward?.Invoke();

            if (_saveLoadManager != null)
                _saveLoadManager.SaveGame();
        });
    }

    private void AddGems(int amount)
    {
        if (_gemsData == null)
        {
            Debug.LogError("ShopActionService: GemsData не назначен.");
            return;
        }

        _gemsData.AddResource(amount, 1f);
    }

    private void AddCoins(int amount)
    {
        if (_coinsData == null)
        {
            Debug.LogError("ShopActionService: CoinsData не назначен.");
            return;
        }

        _coinsData.AddResource(amount, 1f);
    }

    private void BuyPotion(ShopProductConfig product)
    {
        if (_potionEffectService == null)
        {
            Debug.LogError("ShopActionService: PotionEffectService не назначен.");
            return;
        }

        _potionEffectService.Activate(product);
    }
}