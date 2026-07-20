using System;
using UnityEngine;

public class AdminUpgradePurchase : MonoBehaviour, IPurchasable
{
    private const float PurchaseDebounceSeconds = 0.25f;

    [SerializeField] private CoinsData _coinsData;
    [SerializeField] private SaveLoadManager _saveLoadManager;
    [SerializeField] private PurchaseFeedbackPresenter _feedbackPresenter;
    [SerializeField] private TutorialPurchaseGate _tutorialPurchaseGate;

    private AdminWorker _selectedAdmin;
    private bool _isPurchasing;
    private float _nextPurchaseAllowedTime = float.NegativeInfinity;

    public AdminWorker SelectedAdmin => _selectedAdmin;

    public event Action<AdminWorker> OnSelectedAdminChanged;
    public event Action<AdminWorker> OnAdminUpgraded;

    public bool CanBuy()
    {
        if (_isPurchasing ||
            _selectedAdmin == null ||
            _coinsData == null ||
            !CanPassTutorialGate(out _))
            return false;

        if (_selectedAdmin.IsHired == false)
            return false;

        if (_selectedAdmin.CanUpgrade() == false)
            return false;

        int price = _selectedAdmin.GetUpgradePrice();
        return price >= 0 && _coinsData.CurrentCoins >= price;
    }

    public void Buy()
    {
        if (_isPurchasing || Time.unscaledTime < _nextPurchaseAllowedTime)
            return;

        if (_selectedAdmin == null || !_selectedAdmin.IsHired)
        {
            Fail(PurchaseFailureReason.ProductUnavailable);
            return;
        }

        if (!CanPassTutorialGate(out PurchaseFailureReason tutorialReason))
        {
            Fail(tutorialReason);
            return;
        }

        if (!_selectedAdmin.CanUpgrade())
        {
            Fail(PurchaseFailureReason.MaximumReached);
            return;
        }

        if (_coinsData == null)
        {
            Fail(PurchaseFailureReason.TransactionFailed);
            return;
        }

        // ИЗМЕНЕНО: фиксируем цель и цену до списания, чтобы повторный клик
        // или смена выбранного админа не могли провести вторую операцию.
        AdminWorker admin = _selectedAdmin;
        int price = admin.GetUpgradePrice();

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

        int levelBeforePurchase = admin.LevelIndex;
        int coinsBeforePurchase = _coinsData.CurrentCoins;
        _isPurchasing = true;

        try
        {
            if (!_coinsData.TryBuy(price))
            {
                Fail(PurchaseFailureReason.NotEnoughCoins);
                return;
            }

            if (!admin.TryUpgrade())
            {
                _coinsData.AddResource(price, 1f);
                Fail(PurchaseFailureReason.TransactionFailed);
                return;
            }

            // ИЗМЕНЕНО: быстрый двойной click не покупает два уровня подряд.
            _nextPurchaseAllowedTime = Time.unscaledTime + PurchaseDebounceSeconds;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
            Fail(PurchaseFailureReason.TransactionFailed);

            if (admin.LevelIndex == levelBeforePurchase)
            {
                int missingCoins = Mathf.Max(0, coinsBeforePurchase - _coinsData.CurrentCoins);

                if (missingCoins > 0)
                    _coinsData.AddResource(missingCoins, 1f);
            }

            return;
        }
        finally
        {
            _isPurchasing = false;
        }

        InvokeSafely(OnAdminUpgraded, admin);
        _saveLoadManager?.SaveGame();
    }

    public void ClearSelectedAdmin()
    {
        _selectedAdmin = null;
        InvokeSafely(OnSelectedAdminChanged, null);
    }

    public void SelectAdmin(AdminWorker admin)
    {
        if (admin != null && !admin.IsHired)
            admin = null;

        if (_selectedAdmin == admin)
            return;

        _selectedAdmin = admin;
        InvokeSafely(OnSelectedAdminChanged, _selectedAdmin);
    }

    private bool CanPassTutorialGate(out PurchaseFailureReason failureReason)
    {
        if (_tutorialPurchaseGate == null)
        {
            failureReason = PurchaseFailureReason.TransactionFailed;
            return false;
        }

        return _tutorialPurchaseGate.CanPurchase(
            TutorialPurchaseCategory.AdminUpgrade,
            out failureReason);
    }

    private void InvokeSafely(Action<AdminWorker> callback, AdminWorker admin)
    {
        if (callback == null)
            return;

        foreach (Delegate handler in callback.GetInvocationList())
        {
            try
            {
                ((Action<AdminWorker>)handler).Invoke(admin);
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
