using System.Collections;
using UnityEngine;

public class PotionEffectService : MonoBehaviour
{
    [SerializeField] private ResourcesMultiplier _resourcesMultiplier;
    [SerializeField] private SpeedPotionEffectService _speedPotionEffectService;
    [SerializeField] private RatingPotionEffectService _ratingPotionEffectService;

    private Coroutine _coinsPotionCoroutine;

    public void Activate(ShopProductConfig product)
    {
        if (product == null)
            return;

        switch (product.PotionType)
        {
            case PotionType.Coins:
                ActivateCoinsPotion(product.DurationSeconds, product.EffectMultiplier);
                break;

            case PotionType.Speed:
                if (_speedPotionEffectService == null)
                {
                    Debug.LogError("PotionEffectService: не назначен SpeedPotionEffectService.");
                    return;
                }

                _speedPotionEffectService.Activate(product.DurationSeconds, product.EffectMultiplier);
                break;

            case PotionType.Rating:
                if (_ratingPotionEffectService == null)
                {
                    Debug.LogError("PotionEffectService: не назначен RatingPotionEffectService.");
                    return;
                }

                _ratingPotionEffectService.Activate(product.DurationSeconds, product.EffectMultiplier);
                break;
        }
    }

    private void ActivateCoinsPotion(float duration, int multiplier)
    {
        if (_resourcesMultiplier == null)
        {
            Debug.LogError("PotionEffectService: не назначен ResourcesMultiplier.");
            return;
        }

        _resourcesMultiplier.SetMultiplier(ResourceType.Coins, Mathf.Max(1, multiplier));

        if (_coinsPotionCoroutine != null)
            StopCoroutine(_coinsPotionCoroutine);

        _coinsPotionCoroutine = StartCoroutine(ResetCoinsPotionAfter(duration));

        Debug.Log($"Зелье монет активировано: x{multiplier} на {duration} секунд.");
    }

    private IEnumerator ResetCoinsPotionAfter(float duration)
    {
        yield return new WaitForSeconds(Mathf.Max(0.1f, duration));

        if (_resourcesMultiplier != null)
            _resourcesMultiplier.ResetMultiplier(ResourceType.Coins);

        _coinsPotionCoroutine = null;

        Debug.Log("Зелье монет закончилось.");
    }
}
