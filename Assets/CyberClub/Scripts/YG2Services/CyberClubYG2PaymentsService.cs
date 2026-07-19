using System;
using UnityEngine;
using YG;

public class CyberClubYG2PaymentsService : MonoBehaviour
{
    [Header("Yandex product ID")]
    [SerializeField] private string _premiumZoneProductId = "premium_zone_100";

    [Header("Premium")]
    [SerializeField] private PremiumLocationUnlocker _premiumLocationUnlocker;
    [SerializeField] private GemsData _gemsData;
    [SerializeField, Min(0)] private int _premiumGemsReward = 100;

    [Header("Save")]
    [SerializeField] private SaveLoadManager _saveLoadManager;
    [SerializeField] private PurchaseFeedbackPresenter _feedbackPresenter;

    public event Action<string> OnPurchaseSuccess;
    public event Action<string> OnPurchaseFailed;

    private bool _purchaseRequestPending;
    private bool _isHandlingSuccess;

    public string PremiumProductId => _premiumZoneProductId;
    public int PremiumGemsReward => _premiumGemsReward;
    public bool IsPurchasePending => _purchaseRequestPending;

    private void OnEnable()
    {
        YG2.onPurchaseSuccess += HandlePurchaseSuccess;
        YG2.onPurchaseFailed += HandlePurchaseFailed;
    }

    private void Start()
    {
        if (_saveLoadManager == null)
        {
            Debug.LogError("CyberClubYG2PaymentsService: не назначен SaveLoadManager.", this);
            return;
        }

        if (_saveLoadManager.IsLoaded)
            ConsumePurchasesAfterLoad();
        else
            _saveLoadManager.OnGameLoaded += ConsumePurchasesAfterLoad;
    }

    private void OnDisable()
    {
        YG2.onPurchaseSuccess -= HandlePurchaseSuccess;
        YG2.onPurchaseFailed -= HandlePurchaseFailed;

        if (_saveLoadManager != null)
            _saveLoadManager.OnGameLoaded -= ConsumePurchasesAfterLoad;
    }

    public void BuyPremiumLocation()
    {
        if (_purchaseRequestPending ||
            _saveLoadManager == null ||
            !_saveLoadManager.IsLoaded ||
            _premiumLocationUnlocker == null ||
            _premiumLocationUnlocker.IsUnlocked)
        {
            if (_premiumLocationUnlocker != null && _premiumLocationUnlocker.IsUnlocked)
                _feedbackPresenter?.Show(PurchaseFailureReason.ProductUnavailable);
            else if (!_purchaseRequestPending)
                _feedbackPresenter?.Show(PurchaseFailureReason.TransactionFailed);

            return;
        }

        _purchaseRequestPending = true;

        try
        {
            YG2.BuyPayments(_premiumZoneProductId);
        }
        catch (Exception exception)
        {
            _purchaseRequestPending = false;
            Debug.LogException(exception, this);
            _feedbackPresenter?.Show(PurchaseFailureReason.TransactionFailed);
        }
    }

    private void HandlePurchaseSuccess(string purchaseId)
    {
        if (purchaseId != _premiumZoneProductId)
            return;

        _purchaseRequestPending = false;

        if (_isHandlingSuccess ||
            _premiumLocationUnlocker == null ||
            _gemsData == null ||
            _saveLoadManager == null ||
            !_saveLoadManager.IsLoaded)
        {
            return;
        }

        _isHandlingSuccess = true;

        try
        {
            if (!_premiumLocationUnlocker.BonusGranted)
            {
                int gemsBefore = _gemsData.CurrentGems;

                try
                {
                    _gemsData.AddResource(_premiumGemsReward, 1f);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);

                    // ИЗМЕНЕНО: событие UI могло выбросить исключение уже после
                    // фактического начисления; состояние всё равно помечаем завершённым.
                    if (_gemsData.CurrentGems < gemsBefore + _premiumGemsReward)
                        return;
                }

                _premiumLocationUnlocker.TryMarkBonusGranted();
            }

            _premiumLocationUnlocker.UnlockPremiumLocation();
            _saveLoadManager.SaveGame();
            OnPurchaseSuccess?.Invoke(purchaseId);
        }
        finally
        {
            _isHandlingSuccess = false;
        }
    }

    private void HandlePurchaseFailed(string purchaseId)
    {
        if (purchaseId != _premiumZoneProductId)
            return;

        _purchaseRequestPending = false;
        OnPurchaseFailed?.Invoke(purchaseId);
        _feedbackPresenter?.Show(PurchaseFailureReason.TransactionFailed);
        Debug.LogWarning($"Покупка не завершена: {purchaseId}");
    }

    private void ConsumePurchasesAfterLoad()
    {
        _saveLoadManager.OnGameLoaded -= ConsumePurchasesAfterLoad;

        // ИЗМЕНЕНО: consume запускается только после Restore всех save-модулей,
        // поэтому повторный callback видит сохранённый флаг одноразовой награды.
        YG2.ConsumePurchases();
    }
}
