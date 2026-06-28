using UnityEngine;

public class ShopActionService : MonoBehaviour
{
    [Header("Resources")]
    [SerializeField] private GemsData _gemsData;
    [SerializeField] private CoinsData _coinsData;

    [Header("Potion effects")]
    [SerializeField] private PotionEffectService _potionEffectService;

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
                AddGems(product.RewardAmount);
                break;

            case ShopProductActionType.RewardCoins:
                AddCoins(product.RewardAmount);
                break;

            case ShopProductActionType.Unavailable:
                Debug.Log($"Товар {product.name} пока недоступен.");
                break;

            default:
                Debug.LogWarning($"ShopActionService: неизвестный тип действия {product.ActionType}.");
                break;
        }
    }

    private void BuyPotion(ShopProductConfig product)
    {
        if (_gemsData == null)
        {
            Debug.LogError("ShopActionService: не назначен GemsData.");
            return;
        }

        if (_potionEffectService == null)
        {
            Debug.LogError("ShopActionService: не назначен PotionEffectService.");
            return;
        }

        if (_gemsData.TryBuy(product.PriceGems) == false)
        {
            Debug.Log($"Недостаточно гемов для покупки: {product.name}. Нужно: {product.PriceGems}, есть: {_gemsData.CurrentGems}.");
            return;
        }

        _potionEffectService.Activate(product);
        Debug.Log($"Куплено зелье: {product.name} за {product.PriceGems} гемов.");
    }

    private void AddGems(int amount)
    {
        if (_gemsData == null)
        {
            Debug.LogError("ShopActionService: не назначен GemsData.");
            return;
        }

        if (amount <= 0)
        {
            Debug.LogWarning("ShopActionService: награда гемов равна 0.");
            return;
        }

        _gemsData.AddResource(amount, 1f);
        Debug.Log($"+{amount} гемов.");
    }

    private void AddCoins(int amount)
    {
        if (_coinsData == null)
        {
            Debug.LogError("ShopActionService: не назначен CoinsData.");
            return;
        }

        if (amount <= 0)
        {
            Debug.LogWarning("ShopActionService: награда монет равна 0.");
            return;
        }

        _coinsData.AddResource(amount, 1f);
        Debug.Log($"+{amount} монет.");
    }
}
