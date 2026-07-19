using UnityEngine;

public class PremiumLocationSaveModule : MonoBehaviour, ISaveModule
{
    [SerializeField] private PremiumLocationUnlocker _premiumLocationUnlocker;

    public void Capture(GameSaveData saveData)
    {
        if (_premiumLocationUnlocker == null)
            return;

        saveData.PremiumLocation.HasPremiumLocationSave = true;
        saveData.PremiumLocation.IsUnlocked = _premiumLocationUnlocker.IsUnlocked;
        saveData.PremiumLocation.HasBonusGrantState = true;
        saveData.PremiumLocation.BonusGranted = _premiumLocationUnlocker.BonusGranted;
    }

    public void Restore(GameSaveData saveData)
    {
        if (_premiumLocationUnlocker == null)
            return;
        if (saveData.PremiumLocation == null || saveData.PremiumLocation.HasPremiumLocationSave == false)
            return;

        // ИЗМЕНЕНО: старый сейв с уже открытой премиум-зоной считается
        // получившим бонус, чтобы обычная загрузка не начисляла гемы повторно.
        bool bonusGranted = saveData.PremiumLocation.HasBonusGrantState
            ? saveData.PremiumLocation.BonusGranted
            : saveData.PremiumLocation.IsUnlocked;

        _premiumLocationUnlocker.RestoreUnlockedState(
            saveData.PremiumLocation.IsUnlocked,
            bonusGranted);
    }
}

