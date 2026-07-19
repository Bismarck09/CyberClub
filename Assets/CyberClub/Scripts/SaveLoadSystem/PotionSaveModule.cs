using System;
using System.Collections.Generic;
using UnityEngine;

public class PotionSaveModule : MonoBehaviour, ISaveModule
{
    [SerializeField] private PotionEffectService _potionEffectService;
    [SerializeField] private List<ShopProductConfig> _potionProducts = new();

    public void Capture(GameSaveData saveData)
    {
        saveData.Potions ??= new PotionsSaveData();
        saveData.Potions.HasPotionSave = true;
        saveData.Potions.SavedAtUtcTicks = DateTime.UtcNow.Ticks;
        saveData.Potions.ActivePotions.Clear();

        if (_potionEffectService == null)
            return;

        foreach (ActivePotionRuntime potion in _potionEffectService.ActivePotions)
        {
            if (potion == null || potion.RemainingTime <= 0f)
                continue;

            saveData.Potions.ActivePotions.Add(new ActivePotionSaveData
            {
                PotionType = potion.PotionType,
                Duration = potion.Duration,
                RemainingTime = potion.RemainingTime
            });
        }
    }

    public void Restore(GameSaveData saveData)
    {
        PotionsSaveData data = saveData.Potions;

        if (_potionEffectService == null ||
            data == null ||
            !data.HasPotionSave ||
            data.ActivePotions == null)
        {
            return;
        }

        double elapsedSeconds = GetElapsedSeconds(data.SavedAtUtcTicks);
        HashSet<PotionType> restoredTypes = new();

        foreach (ActivePotionSaveData savedPotion in data.ActivePotions)
        {
            if (savedPotion == null || !restoredTypes.Add(savedPotion.PotionType))
                continue;

            float remainingTime = Mathf.Max(
                0f,
                savedPotion.RemainingTime - (float)elapsedSeconds);

            if (remainingTime <= 0f)
                continue;

            ShopProductConfig product = _potionProducts.Find(
                candidate => candidate != null && candidate.PotionType == savedPotion.PotionType);

            if (product == null)
            {
                Debug.LogWarning($"PotionSaveModule: не найден товар для {savedPotion.PotionType}.", this);
                continue;
            }

            // ИЗМЕНЕНО: таймер продолжает идти вне игры и восстанавливается
            // ровно один раз только при положительном остатке.
            _potionEffectService.TryRestore(
                product,
                Mathf.Max(0.1f, savedPotion.Duration),
                remainingTime);
        }
    }

    private static double GetElapsedSeconds(long savedAtUtcTicks)
    {
        if (savedAtUtcTicks <= 0L || savedAtUtcTicks > DateTime.MaxValue.Ticks)
            return 0d;

        long elapsedTicks = Math.Max(0L, DateTime.UtcNow.Ticks - savedAtUtcTicks);
        return TimeSpan.FromTicks(elapsedTicks).TotalSeconds;
    }
}
