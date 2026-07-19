using System;
using System.Collections.Generic;
using UnityEngine;

public class AdminPurchase : MonoBehaviour, IPurchasable
{
    private const float PurchaseDebounceSeconds = 0.25f;

    [SerializeField] private CoinsData _coinsData;
    [SerializeField] private List<AdminWorker> _admins;
    [SerializeField] private SaveLoadManager _saveLoadManager;
    [SerializeField] private CyberClubTutorialManager _tutorialManager;
    [SerializeField] private PurchaseFeedbackPresenter _feedbackPresenter;

    private bool _isPurchasing;
    private float _nextPurchaseAllowedTime = float.NegativeInfinity;

    public event Action OnAdminPurchased;
    public event Action OnAdminStateChanged;

    public bool IsHiringLockedByTutorial =>
        _tutorialManager != null && !_tutorialManager.CanHireAdditionalAdmins;

    private void OnEnable()
    {
        if (_coinsData != null)
            _coinsData.OnCoinsChanged += HandleCoinsChanged;

        if (_tutorialManager != null)
            _tutorialManager.OnTutorialStateChanged += HandleTutorialStateChanged;

        if (_admins == null)
            return;

        foreach (AdminWorker admin in _admins)
        {
            if (admin != null)
                admin.OnChanged += HandleAdminChanged;
        }
    }

    private void OnDisable()
    {
        if (_coinsData != null)
            _coinsData.OnCoinsChanged -= HandleCoinsChanged;

        if (_tutorialManager != null)
            _tutorialManager.OnTutorialStateChanged -= HandleTutorialStateChanged;

        if (_admins == null)
            return;

        foreach (AdminWorker admin in _admins)
        {
            if (admin != null)
                admin.OnChanged -= HandleAdminChanged;
        }
    }

    public bool CanBuy()
    {
        if (_isPurchasing || IsHiringLockedByTutorial)
            return false;

        AdminWorker admin = GetNextNotHiredAdmin();

        if (admin == null || _coinsData == null)
            return false;

        return _coinsData.CurrentCoins >= admin.HirePrice;
    }

    public void Buy()
    {
        // ИЗМЕНЕНО: блокируем второй UI-click сразу после успешного найма,
        // но не делаем CanBuy зависимым от таймера, чтобы кнопка сама вернулась в активное состояние.
        if (_isPurchasing || Time.unscaledTime < _nextPurchaseAllowedTime)
            return;

        if (IsHiringLockedByTutorial)
        {
            Fail(PurchaseFailureReason.LockedByTutorial);
            return;
        }

        AdminWorker admin = GetNextNotHiredAdmin();

        if (admin == null)
        {
            Fail(PurchaseFailureReason.MaximumReached);
            return;
        }

        if (_coinsData == null)
        {
            Fail(PurchaseFailureReason.TransactionFailed);
            return;
        }

        int price = admin.HirePrice;

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

        int coinsBeforePurchase = _coinsData.CurrentCoins;
        _isPurchasing = true;

        try
        {
            if (!_coinsData.TryBuy(price))
            {
                Fail(PurchaseFailureReason.NotEnoughCoins);
                return;
            }

            if (!admin.TryHire())
            {
                _coinsData.AddResource(price, 1f);
                Fail(PurchaseFailureReason.TransactionFailed);
                return;
            }

            _nextPurchaseAllowedTime = Time.unscaledTime + PurchaseDebounceSeconds;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
            Fail(PurchaseFailureReason.TransactionFailed);

            // ИЗМЕНЕНО: возвращаем деньги только если найм действительно не состоялся.
            if (!admin.IsHired)
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

        // ИЗМЕНЕНО: уведомления и сохранение идут после завершённой транзакции.
        InvokeSafely(OnAdminPurchased);
        NotifyStateChanged();
        _saveLoadManager?.SaveGame();
    }

    public AdminWorker GetNextNotHiredAdmin()
    {
        if (_admins == null)
            return null;

        foreach (AdminWorker admin in _admins)
        {
            if (admin != null && admin.IsHired == false)
                return admin;
        }

        return null;
    }

    private void HandleAdminChanged(AdminWorker unusedAdmin)
    {
        NotifyStateChanged();
    }

    private void HandleCoinsChanged(int unusedDelta)
    {
        NotifyStateChanged();
    }

    private void HandleTutorialStateChanged()
    {
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        InvokeSafely(OnAdminStateChanged);
    }

    private void InvokeSafely(Action callback)
    {
        if (callback == null)
            return;

        foreach (Delegate handler in callback.GetInvocationList())
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
