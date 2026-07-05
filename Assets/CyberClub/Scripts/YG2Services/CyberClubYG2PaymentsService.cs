using System;
using UnityEngine;
using YG;

public class CyberClubYG2PaymentsService : MonoBehaviour
{
    [Header("Yandex product ID")]
    [SerializeField] private string _premiumZoneProductId = "premium_zone_100";

    [Header("Premium")]
    [SerializeField] private PremiumLocationUnlocker _premiumLocationUnlocker;

    [Header("Save")]
    [SerializeField] private SaveLoadManager _saveLoadManager;

    public event Action<string> OnPurchaseSuccess;
    public event Action<string> OnPurchaseFailed;

    private void OnEnable()
    {
        YG2.onPurchaseSuccess += HandlePurchaseSuccess;
        YG2.onPurchaseFailed += HandlePurchaseFailed;
    }

    private void Start()
    {
        YG2.ConsumePurchases();
    }

    private void OnDisable()
    {
        YG2.onPurchaseSuccess -= HandlePurchaseSuccess;
        YG2.onPurchaseFailed -= HandlePurchaseFailed;
    }

    public void BuyPremiumLocation() => YG2.BuyPayments(_premiumZoneProductId);

    private void HandlePurchaseSuccess(string purchaseId)
    {
        if (purchaseId != _premiumZoneProductId)
            return;

        if (_premiumLocationUnlocker != null)
            _premiumLocationUnlocker.UnlockPremiumLocation();

        if (_saveLoadManager != null)
            _saveLoadManager.SaveGame();

        OnPurchaseSuccess?.Invoke(purchaseId);
    }

    private void HandlePurchaseFailed(string purchaseId)
    {
        if (purchaseId != _premiumZoneProductId)
            return;

        OnPurchaseFailed?.Invoke(purchaseId);
        Debug.LogWarning($"Покупка не завершена: {purchaseId}");
    }
}