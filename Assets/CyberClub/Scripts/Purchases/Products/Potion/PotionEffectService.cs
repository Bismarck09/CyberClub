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
            InvokeSafely(OnPotionUpdated, potion);

            if (potion.RemainingTime <= 0f)
                _expiredPotions.Add(pair.Key);
        }

        foreach (PotionType potionType in _expiredPotions)
            EndPotion(potionType);
    }

    public bool TryActivate(ShopProductConfig product)
    {
        if (!CanApply(product))
            return false;

        ApplyEffect(product);

        if (_activePotions.TryGetValue(product.PotionType, out ActivePotionRuntime activePotion))
        {
            activePotion.Restart(product);
            InvokeSafely(OnPotionStarted, activePotion);
            Debug.Log($"Зелье обновлено: {product.name}. Таймер сброшен до {activePotion.Duration} сек.");
            return true;
        }

        ActivePotionRuntime newPotion = new ActivePotionRuntime(product);
        _activePotions.Add(product.PotionType, newPotion);
        InvokeSafely(OnPotionStarted, newPotion);

        Debug.Log($"Зелье активировано: {product.name}. Длительность: {newPotion.Duration} сек.");
        return true;
    }

    public bool TryRestore(
        ShopProductConfig product,
        float duration,
        float remainingTime)
    {
        if (!CanApply(product) || remainingTime <= 0f)
            return false;

        ApplyEffect(product);

        ActivePotionRuntime runtime = new ActivePotionRuntime(product);
        runtime.Restore(product, duration, remainingTime);
        _activePotions[product.PotionType] = runtime;
        InvokeSafely(OnPotionStarted, runtime);
        return true;
    }

    public bool IsActive(PotionType potionType)
    {
        return _activePotions.ContainsKey(potionType);
    }

    private bool CanApply(ShopProductConfig product)
    {
        if (product == null ||
            product.ActionType != ShopProductActionType.Potion ||
            product.DurationSeconds <= 0f)
        {
            return false;
        }

        switch (product.PotionType)
        {
            case PotionType.Coins:
                return _resourcesMultiplier != null;
            case PotionType.Speed:
                return _speedPotionEffectService != null;
            case PotionType.Rating:
                return _ratingPotionEffectService != null;
            default:
                return false;
        }
    }

    private void ApplyEffect(ShopProductConfig product)
    {
        switch (product.PotionType)
        {
            case PotionType.Coins:
                _resourcesMultiplier.SetMultiplier(ResourceType.Coins, Mathf.Max(1, product.EffectMultiplier));
                break;

            case PotionType.Speed:
                _speedPotionEffectService.Apply(product.EffectMultiplier);
                break;

            case PotionType.Rating:
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
        InvokeSafely(OnPotionEnded, potion);

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

    private void InvokeSafely(
        Action<ActivePotionRuntime> callback,
        ActivePotionRuntime potion)
    {
        if (callback == null)
            return;

        foreach (Delegate handler in callback.GetInvocationList())
        {
            try
            {
                ((Action<ActivePotionRuntime>)handler).Invoke(potion);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }
    }
}
