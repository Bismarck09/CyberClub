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
                ActivateCoinsPotion(product.DurationSeconds, product.Multiplier);
                break;

            case PotionType.Speed:
                if (_speedPotionEffectService != null)
                    _speedPotionEffectService.Activate(product.DurationSeconds, product.Multiplier);
                break;

            case PotionType.Rating:
                if (_ratingPotionEffectService != null)
                    _ratingPotionEffectService.Activate(product.DurationSeconds, product.Multiplier);
                break;
        }
    }

    private void ActivateCoinsPotion(float duration, float multiplier)
    {
        if (_resourcesMultiplier == null)
        {
            Debug.LogError("PotionEffectService: не назначен ResourcesMultiplier.");
            return;
        }

        int roundedMultiplier = Mathf.Max(1, Mathf.RoundToInt(multiplier));
        _resourcesMultiplier.SetMultiplier(ResourceType.Coins, roundedMultiplier);

        if (_coinsPotionCoroutine != null)
            StopCoroutine(_coinsPotionCoroutine);

        _coinsPotionCoroutine = StartCoroutine(ResetCoinsMultiplierAfter(duration));
    }

    private IEnumerator ResetCoinsMultiplierAfter(float duration)
    {
        yield return new WaitForSeconds(Mathf.Max(0.1f, duration));

        if (_resourcesMultiplier != null)
            _resourcesMultiplier.ResetMultiplier(ResourceType.Coins);

        _coinsPotionCoroutine = null;
    }
}
