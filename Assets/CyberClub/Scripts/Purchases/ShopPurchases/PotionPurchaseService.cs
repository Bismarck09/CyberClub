using System;
using UnityEngine;

public class PotionPurchaseService : MonoBehaviour
{
    [SerializeField] private GemsData _gemsData;
    [SerializeField] private PotionEffectService _potionEffectService;

    public event Action<ShopProductConfig> OnPotionPurchased;
    public event Action<ShopProductConfig> OnNotEnoughGems;

    public bool TryBuy(ShopProductConfig product)
    {
        if (product == null)
            return false;

        if (product.Category != ShopProductCategory.Potions)
        {
            Debug.LogWarning($"PotionPurchaseService: товар {product.name} не является зельем.");
            return false;
        }

        if (_gemsData == null)
        {
            Debug.LogError("PotionPurchaseService: не назначен GemsData.");
            return false;
        }

        if (_potionEffectService == null)
        {
            Debug.LogError("PotionPurchaseService: не назначен PotionEffectService.");
            return false;
        }

        if (_gemsData.TryBuy(product.PriceGems) == false)
        {
            OnNotEnoughGems?.Invoke(product);
            return false;
        }

        _potionEffectService.Activate(product);
        OnPotionPurchased?.Invoke(product);
        return true;
    }
}