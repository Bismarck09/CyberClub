using System;
using UnityEngine;

public class PotionPurchaseService : MonoBehaviour
{
    private const float PurchaseDebounceSeconds = 0.25f;

    [SerializeField] private GemsData _gemsData;
    [SerializeField] private PotionEffectService _potionEffectService;
    [SerializeField] private SaveLoadManager _saveLoadManager;
    [SerializeField] private PurchaseFeedbackPresenter _feedbackPresenter;
    [SerializeField] private TutorialPurchaseGate _tutorialPurchaseGate;

    private bool _isPurchasing;
    private float _nextPurchaseAllowedTime = float.NegativeInfinity;

    public event Action<ShopProductConfig> OnPotionPurchased;
    public event Action<ShopProductConfig> OnNotEnoughGems;

    public bool TryBuy(ShopProductConfig product)
    {
        if (_isPurchasing ||
            Time.unscaledTime < _nextPurchaseAllowedTime ||
            product == null)
        {
            return false;
        }

        if (product.ActionType != ShopProductActionType.Potion)
        {
            Debug.LogWarning($"PotionPurchaseService: товар {product.name} не является зельем.");
            Fail(PurchaseFailureReason.ProductUnavailable);
            return false;
        }

        if (!CanPassTutorialGate(out PurchaseFailureReason tutorialReason))
        {
            Fail(tutorialReason);
            return false;
        }

        if (_gemsData == null)
        {
            Debug.LogError("PotionPurchaseService: не назначен GemsData.");
            Fail(PurchaseFailureReason.TransactionFailed);
            return false;
        }

        if (_potionEffectService == null)
        {
            Debug.LogError("PotionPurchaseService: не назначен PotionEffectService.");
            Fail(PurchaseFailureReason.TransactionFailed);
            return false;
        }

        int price = Mathf.Max(0, product.PriceGems);

        if (_gemsData.CurrentGems < price)
        {
            Debug.Log($"Недостаточно гемов для покупки {product.name}. Нужно: {product.PriceGems}, есть: {_gemsData.CurrentGems}.");
            OnNotEnoughGems?.Invoke(product);
            Fail(PurchaseFailureReason.NotEnoughGems);
            return false;
        }

        int gemsBeforePurchase = _gemsData.CurrentGems;
        _isPurchasing = true;
        bool effectActivated = false;

        try
        {
            if (!_gemsData.TryBuy(price))
            {
                Fail(PurchaseFailureReason.NotEnoughGems);
                return false;
            }

            // ИЗМЕНЕНО: только эта транзакционная точка может активировать
            // покупаемое за гемы зелье.
            if (!_potionEffectService.TryActivate(product))
            {
                _gemsData.AddResource(price, 1f);
                Fail(PurchaseFailureReason.TransactionFailed);
                return false;
            }

            effectActivated = true;
            // ИЗМЕНЕНО: повторное touch/click-событие не запускает вторую покупку.
            _nextPurchaseAllowedTime = Time.unscaledTime + PurchaseDebounceSeconds;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
            Fail(PurchaseFailureReason.TransactionFailed);

            if (!effectActivated)
            {
                int missingGems = Mathf.Max(0, gemsBeforePurchase - _gemsData.CurrentGems);

                if (missingGems > 0)
                    _gemsData.AddResource(missingGems, 1f);
            }

            return false;
        }
        finally
        {
            _isPurchasing = false;
        }

        try
        {
            OnPotionPurchased?.Invoke(product);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
        }

        _saveLoadManager?.SaveGame();

        Debug.Log($"Куплено зелье: {product.name}.");
        return true;
    }

    private void Fail(PurchaseFailureReason reason)
    {
        _feedbackPresenter?.Show(reason);
    }

    private bool CanPassTutorialGate(out PurchaseFailureReason failureReason)
    {
        if (_tutorialPurchaseGate == null)
        {
            failureReason = PurchaseFailureReason.TransactionFailed;
            return false;
        }

        return _tutorialPurchaseGate.CanPurchase(
            TutorialPurchaseCategory.Potion,
            out failureReason);
    }
}
