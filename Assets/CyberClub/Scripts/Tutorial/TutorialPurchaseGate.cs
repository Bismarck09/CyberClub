using System;
using UnityEngine;

public enum TutorialPurchaseCategory
{
    Device,
    Interior,
    AdminHire,
    AdminUpgrade,
    Zone,
    Potion
}

[DisallowMultipleComponent]
public class TutorialPurchaseGate : MonoBehaviour
{
    [SerializeField] private CyberClubTutorialManager _tutorialManager;

    public event Action OnGateStateChanged;

    private void Awake()
    {
        if (_tutorialManager == null)
        {
            Debug.LogError(
                $"TutorialPurchaseGate: поле {nameof(_tutorialManager)} не назначено на GameObject '{name}'.",
                this);
        }
    }

    private void OnEnable()
    {
        if (_tutorialManager != null)
            _tutorialManager.OnTutorialStateChanged += NotifyGateStateChanged;
    }

    private void OnDisable()
    {
        if (_tutorialManager != null)
            _tutorialManager.OnTutorialStateChanged -= NotifyGateStateChanged;
    }

    public bool CanPurchase(
        TutorialPurchaseCategory category,
        out PurchaseFailureReason blockedReason)
    {
        return CanPurchase(category, null, out blockedReason);
    }

    public bool CanPurchase(
        TutorialPurchaseCategory category,
        ZoneInformation zone,
        out PurchaseFailureReason blockedReason)
    {
        if (_tutorialManager == null)
        {
            blockedReason = PurchaseFailureReason.TransactionFailed;
            return false;
        }

        if (!_tutorialManager.HasFirstComputerPurchased)
        {
            bool isFirstAllowedDevice = category == TutorialPurchaseCategory.Device &&
                _tutorialManager.IsFirstRoom(zone);
            bool isRequiredFirstRoomUnlock = category == TutorialPurchaseCategory.Zone &&
                _tutorialManager.IsFirstRoom(zone);
            bool allowed = isFirstAllowedDevice || isRequiredFirstRoomUnlock;

            blockedReason = allowed
                ? PurchaseFailureReason.None
                : PurchaseFailureReason.FirstComputerRequired;
            return allowed;
        }

        if (_tutorialManager.IsBasicTutorialCompleted)
        {
            blockedReason = PurchaseFailureReason.None;
            return true;
        }

        switch (category)
        {
            case TutorialPurchaseCategory.Device:
                blockedReason = PurchaseFailureReason.None;
                return true;

            case TutorialPurchaseCategory.Interior:
                bool interiorAllowed = _tutorialManager.IsInteriorPurchaseAvailable;
                blockedReason = interiorAllowed
                    ? PurchaseFailureReason.None
                    : PurchaseFailureReason.InteriorTutorialRequired;
                return interiorAllowed;

            case TutorialPurchaseCategory.AdminHire:
            case TutorialPurchaseCategory.AdminUpgrade:
                bool adminAllowed = _tutorialManager.AreAdminPurchasesAvailable;
                blockedReason = adminAllowed
                    ? PurchaseFailureReason.None
                    : PurchaseFailureReason.TutorialStageIncomplete;
                return adminAllowed;

            case TutorialPurchaseCategory.Zone:
            case TutorialPurchaseCategory.Potion:
                blockedReason = PurchaseFailureReason.TutorialStageIncomplete;
                return false;

            default:
                blockedReason = PurchaseFailureReason.TransactionFailed;
                return false;
        }
    }

    private void NotifyGateStateChanged()
    {
        OnGateStateChanged?.Invoke();
    }
}
