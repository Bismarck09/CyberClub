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

        if (product.ActionType != ShopProductActionType.Potion)
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
            Debug.Log($"Недостаточно гемов для покупки {product.name}. Нужно: {product.PriceGems}, есть: {_gemsData.CurrentGems}.");
            OnNotEnoughGems?.Invoke(product);
            return false;
        }

        _potionEffectService.Activate(product);
        OnPotionPurchased?.Invoke(product);

        Debug.Log($"Куплено зелье: {product.name}.");
        return true;
    }
}