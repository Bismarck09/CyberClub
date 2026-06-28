using System;
using System.Collections.Generic;
using UnityEngine;

public class PotionEffectService : MonoBehaviour
{
    [SerializeField] private ResourcesMultiplier _resourcesMultiplier;
    [SerializeField] private SpeedPotionEffectService _speedPotionEffectService;
    [SerializeField] private RatingPotionEffectService _ratingPotionEffectService;

    private readonly Dictionary<PotionType, ActivePotionRuntime> _activePotions = new();
    private readonly List<PotionType> _expiredPotions = new();

    public event Action<ActivePotionRuntime> OnPotionStarted;
    public event Action<ActivePotionRuntime> OnPotionUpdated;
    public event Action<ActivePotionRuntime> OnPotionEnded;

    public IReadOnlyCollection<ActivePotionRuntime> ActivePotions => _activePotions.Values;

    private void Update()
    {
        if (_activePotions.Count == 0)
            return;

        _expiredPotions.Clear();

        foreach (KeyValuePair<PotionType, ActivePotionRuntime> pair in _activePotions)
        {
            ActivePotionRuntime potion = pair.Value;
            potion.Tick(Time.deltaTime);
            OnPotionUpdated?.Invoke(potion);

            if (potion.RemainingTime <= 0f)
                _expiredPotions.Add(pair.Key);
        }

        foreach (PotionType potionType in _expiredPotions)
            EndPotion(potionType);
    }

    public void Activate(ShopProductConfig product)
    {
        if (product == null)
            return;

        if (product.ActionType != ShopProductActionType.Potion)
        {
            Debug.LogWarning($"PotionEffectService: товар {product.name} не является зельем.");
            return;
        }

        ApplyEffect(product);

        if (_activePotions.TryGetValue(product.PotionType, out ActivePotionRuntime activePotion))
        {
            activePotion.Restart(product);
            OnPotionStarted?.Invoke(activePotion);
            Debug.Log($"Зелье обновлено: {product.name}. Таймер сброшен до {activePotion.Duration} сек.");
            return;
        }

        ActivePotionRuntime newPotion = new ActivePotionRuntime(product);
        _activePotions.Add(product.PotionType, newPotion);
        OnPotionStarted?.Invoke(newPotion);

        Debug.Log($"Зелье активировано: {product.name}. Длительность: {newPotion.Duration} сек.");
    }

    private void ApplyEffect(ShopProductConfig product)
    {
        switch (product.PotionType)
        {
            case PotionType.Coins:
                if (_resourcesMultiplier == null)
                {
                    Debug.LogError("PotionEffectService: не назначен ResourcesMultiplier.");
                    return;
                }

                _resourcesMultiplier.SetMultiplier(ResourceType.Coins, Mathf.Max(1, product.EffectMultiplier));
                break;

            case PotionType.Speed:
                if (_speedPotionEffectService == null)
                {
                    Debug.LogError("PotionEffectService: не назначен SpeedPotionEffectService.");
                    return;
                }

                _speedPotionEffectService.Apply(product.EffectMultiplier);
                break;

            case PotionType.Rating:
                if (_ratingPotionEffectService == null)
                {
                    Debug.LogError("PotionEffectService: не назначен RatingPotionEffectService.");
                    return;
                }

                _ratingPotionEffectService.Apply(product.EffectMultiplier);
                break;
        }
    }

    private void EndPotion(PotionType potionType)
    {
        if (!_activePotions.TryGetValue(potionType, out ActivePotionRuntime potion))
            return;

        ResetEffect(potionType);
        _activePotions.Remove(potionType);
        OnPotionEnded?.Invoke(potion);

        Debug.Log($"Зелье закончилось: {potion.Product.name}.");
    }

    private void ResetEffect(PotionType potionType)
    {
        switch (potionType)
        {
            case PotionType.Coins:
                if (_resourcesMultiplier != null)
                    _resourcesMultiplier.ResetMultiplier(ResourceType.Coins);
                break;

            case PotionType.Speed:
                if (_speedPotionEffectService != null)
                    _speedPotionEffectService.ResetEffect();
                break;

            case PotionType.Rating:
                if (_ratingPotionEffectService != null)
                    _ratingPotionEffectService.ResetEffect();
                break;
        }
    }

    private void OnDestroy()
    {
        foreach (PotionType potionType in new List<PotionType>(_activePotions.Keys))
            ResetEffect(potionType);

        _activePotions.Clear();
    }
}
