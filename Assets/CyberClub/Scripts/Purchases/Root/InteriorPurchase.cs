using System;
using UnityEngine;

public class InteriorPurchase : MonoBehaviour, IPurchasable
{
    private const float PurchaseDebounceSeconds = 0.25f;

    [SerializeField] private ZoneSwitcher _zoneSwitcher;
    [SerializeField] private CoinsData _coinsData;
    [SerializeField] private SaveLoadManager _saveLoadManager;
    [SerializeField] private PurchaseFeedbackPresenter _feedbackPresenter;
    [SerializeField] private TutorialPurchaseGate _tutorialPurchaseGate;

    private InteriorData _interiorData;
    private bool _isPurchasing;
    private float _nextPurchaseAllowedTime = float.NegativeInfinity;

    public event Action<InteriorData> OnInteriorPurchase;

    private void OnEnable()
    {
        if (_zoneSwitcher != null)
            _zoneSwitcher.OnZoneChanged += ChangeInteriorData;
    }

    private void OnDisable()
    {
        if (_zoneSwitcher != null)
            _zoneSwitcher.OnZoneChanged -= ChangeInteriorData;
    }

    public bool CanBuy()
    {
        if (_isPurchasing ||
            _interiorData == null ||
            _coinsData == null ||
            !CanPassTutorialGate(out _))
            return false;

        if (_interiorData.IsMaxPurchased)
            return false;

        return _coinsData.CurrentCoins >= _interiorData.InteriorsPrice;
    }

    public void Buy()
    {
        if (_isPurchasing || Time.unscaledTime < _nextPurchaseAllowedTime)
            return;

        InteriorData interior = _interiorData;

        if (interior == null)
        {
            Fail(PurchaseFailureReason.ProductUnavailable);
            return;
        }

        if (!CanPassTutorialGate(out PurchaseFailureReason tutorialReason))
        {
            Fail(tutorialReason);
            return;
        }

        if (interior.IsMaxPurchased)
        {
            Fail(PurchaseFailureReason.MaximumReached);
            return;
        }

        if (_coinsData == null)
        {
            Fail(PurchaseFailureReason.TransactionFailed);
            return;
        }

        int price = interior.InteriorsPrice;

        if (price < 0)
        {
            Fail(PurchaseFailureReason.TransactionFailed);
            return;
        }

        if (_coinsData.CurrentCoins < price)
        {
            Fail(PurchaseFailureReason.NotEnoughCoins);
            return;
        }

        int levelBefore = interior.CurrentBoughtInteriorObjects;
        int coinsBefore = _coinsData.CurrentCoins;
        bool completed = false;
        _isPurchasing = true;

        try
        {
            if (!_coinsData.TryBuy(price))
            {
                Fail(PurchaseFailureReason.NotEnoughCoins);
                return;
            }

            if (!interior.BuyInterior())
            {
                Fail(PurchaseFailureReason.TransactionFailed);
                return;
            }

            completed = true;
            _nextPurchaseAllowedTime = Time.unscaledTime + PurchaseDebounceSeconds;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
            Fail(PurchaseFailureReason.TransactionFailed);
        }
        finally
        {
            if (!completed)
            {
                if (interior.CurrentBoughtInteriorObjects != levelBefore)
                    interior.RestoreBoughtInteriorObjects(levelBefore);

                int missingCoins = Mathf.Max(0, coinsBefore - _coinsData.CurrentCoins);

                if (missingCoins > 0)
                    _coinsData.AddResource(missingCoins, 1f);
            }

            _isPurchasing = false;
        }

        if (!completed)
            return;

        _saveLoadManager?.SaveGame();
        InvokeInteriorPurchasedSafely(interior);
    }

    private void ChangeInteriorData(ZoneInformation zoneInformation)
    {
        _interiorData = zoneInformation != null ? zoneInformation.Interior : null;
    }

    private bool CanPassTutorialGate(out PurchaseFailureReason failureReason)
    {
        if (_tutorialPurchaseGate == null)
        {
            failureReason = PurchaseFailureReason.TransactionFailed;
            return false;
        }

        return _tutorialPurchaseGate.CanPurchase(
            TutorialPurchaseCategory.Interior,
            out failureReason);
    }

    private void Fail(PurchaseFailureReason reason)
    {
        _feedbackPresenter?.Show(reason);
    }

    private void InvokeInteriorPurchasedSafely(InteriorData interior)
    {
        if (OnInteriorPurchase == null)
            return;

        foreach (Delegate handler in OnInteriorPurchase.GetInvocationList())
        {
            try
            {
                ((Action<InteriorData>)handler).Invoke(interior);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }
    }
}
