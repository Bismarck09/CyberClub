using System;
using UnityEngine;

public class ZonePurchase : MonoBehaviour
{
    private const float PurchaseDebounceSeconds = 0.25f;

    [SerializeField] private CoinsData _coinsData;
    [SerializeField] private SaveLoadManager _saveLoadManager;
    [SerializeField] private LocationPurchaseDialog _purchaseDialog;
    [SerializeField] private PurchaseFeedbackPresenter _feedbackPresenter;
    private bool _isPurchasing;
    private float _nextPurchaseAllowedTime = float.NegativeInfinity;

    // Событие можно оставить для квестов, звука и аналитики.
    // BarrierDissolve больше на него не подписывается.
    public event Action OnZonePurchased;

    public void Buy(ZonePurchaseConfig config)
    {
        if (config == null)
        {
            Fail(PurchaseFailureReason.ProductUnavailable);
            return;
        }

        if (_purchaseDialog == null)
        {
            Debug.LogError("ZonePurchase: не назначено окно подтверждения покупки.", this);
            Fail(PurchaseFailureReason.TransactionFailed);
            return;
        }

        _purchaseDialog.OpenOrdinary(config);
    }

    public bool TryBuyConfirmed(ZonePurchaseConfig config)
    {
        if (_isPurchasing || Time.unscaledTime < _nextPurchaseAllowedTime)
            return false;

        PurchaseFailureReason failureReason = GetFailureReason(config);

        if (failureReason != PurchaseFailureReason.None)
        {
            Fail(failureReason);
            return false;
        }

        if (!config.TryBeginPurchase())
        {
            Fail(PurchaseFailureReason.ProductUnavailable);
            return false;
        }

        BarrierDissolve barrier =
            config.GetComponent<BarrierDissolve>();

        int price = config.ZonePrice;
        int coinsBeforePurchase = _coinsData.CurrentCoins;

        // ИЗМЕНЕНО: списание происходит ровно один раз
        // и только после полной проверки объекта.
        _isPurchasing = true;
        bool unlockStarted = false;

        try
        {
            if (!_coinsData.TryBuy(price))
            {
                Fail(PurchaseFailureReason.NotEnoughCoins);
                return false;
            }

            unlockStarted = barrier.TryUnlock();

            if (!unlockStarted)
            {
                Fail(PurchaseFailureReason.TransactionFailed);
                return false;
            }

            config.CommitUnlockedState();
            // ИЗМЕНЕНО: один быстрый жест не может купить две разные зоны подряд.
            _nextPurchaseAllowedTime = Time.unscaledTime + PurchaseDebounceSeconds;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"ZonePurchase: ошибка покупки {config.name}: {exception.Message}",
                this);
            Fail(PurchaseFailureReason.TransactionFailed);
        }
        finally
        {
            if (!unlockStarted)
            {
                config.CancelPurchase();
            }

            _isPurchasing = false;

            if (!unlockStarted)
            {
                int missingCoins = Mathf.Max(0, coinsBeforePurchase - _coinsData.CurrentCoins);

                if (missingCoins > 0)
                    _coinsData.AddResource(missingCoins, 1f);
            }
        }

        if (!unlockStarted)
            return false;

        // ИЗМЕНЕНО: логическое состояние фиксируется и сохраняется сразу после
        // успешного старта открытия, не дожидаясь двухсекундной анимации.
        InvokeZonePurchasedSafely();

        _saveLoadManager?.SaveGame();
        return true;
    }

    // ИЗМЕНЕНО: чистая проверка без изменения CoinsData.
    public bool CanBuy(ZonePurchaseConfig config)
    {
        return !_isPurchasing && GetFailureReason(config) == PurchaseFailureReason.None;
    }

    public PurchaseFailureReason GetFailureReason(ZonePurchaseConfig config)
    {
        if (config == null || _coinsData == null)
            return PurchaseFailureReason.TransactionFailed;

        if (config.IsUnlocked ||
            config.BarrierObject == null ||
            !config.BarrierObject.activeSelf)
        {
            return PurchaseFailureReason.ProductUnavailable;
        }

        BarrierDissolve barrier =
            config.GetComponent<BarrierDissolve>();

        if (barrier == null || !barrier.CanUnlock)
            return PurchaseFailureReason.ProductUnavailable;

        if (config.ZonePrice < 0)
            return PurchaseFailureReason.TransactionFailed;

        if (_coinsData.CurrentCoins < config.ZonePrice)
            return PurchaseFailureReason.NotEnoughCoins;

        return PurchaseFailureReason.None;
    }

    private void InvokeZonePurchasedSafely()
    {
        if (OnZonePurchased == null)
            return;

        foreach (Delegate handler in OnZonePurchased.GetInvocationList())
        {
            try
            {
                ((Action)handler).Invoke();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }
    }

    private void Fail(PurchaseFailureReason reason)
    {
        _feedbackPresenter?.Show(reason);
    }
}
