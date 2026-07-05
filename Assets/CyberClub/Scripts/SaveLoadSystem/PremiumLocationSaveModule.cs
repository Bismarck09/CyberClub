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
    }

    public void Restore(GameSaveData saveData)
    {
        if (_premiumLocationUnlocker == null)
            return;
        if (saveData.PremiumLocation == null || saveData.PremiumLocation.HasPremiumLocationSave == false)
            return;

        _premiumLocationUnlocker.RestoreUnlockedState(saveData.PremiumLocation.IsUnlocked);
    }
}

