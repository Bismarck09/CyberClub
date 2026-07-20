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

        // Retain legacy fields so the JSON shape remains backward-compatible.
        saveData.PremiumLocation.HasBonusGrantState = true;
        saveData.PremiumLocation.BonusGranted = true;
    }

    public void Restore(GameSaveData saveData)
    {
        if (_premiumLocationUnlocker == null)
            return;
        if (saveData.PremiumLocation == null || !saveData.PremiumLocation.HasPremiumLocationSave)
            return;

        _premiumLocationUnlocker.RestoreUnlockedState(saveData.PremiumLocation.IsUnlocked);
    }
}
