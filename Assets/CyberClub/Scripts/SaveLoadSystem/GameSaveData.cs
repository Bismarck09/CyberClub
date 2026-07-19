using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveData
{
    public int Version = 5;

    public ResourceSaveData Resources = new();
    public RatingSaveData Rating = new();

    public List<ZoneSaveData> Zones = new();
    public List<AdminSaveData> Admins = new();

    public SettingsSaveData Settings = new();

    public QuestSaveData Quests = new();
    public TutorialSaveData Tutorial = new();

    // Нужно для YG2-премиум локации. Если пока не используешь — просто останется пустым.
    public PremiumLocationSaveData PremiumLocation = new();
    public PotionsSaveData Potions = new();
}

[Serializable]
public class ResourceSaveData
{
    public int Coins;
    public int Gems;
}

[Serializable]
public class RatingSaveData
{
    public float CurrentRating;
}

[Serializable]
public class ZoneSaveData
{
    public string Id;
    public bool IsUnlocked;
    public int PurchasedDevices;
    public int PurchasedInteriorObjects;
}

[Serializable]
public class AdminSaveData
{
    public string Id;
    public bool IsHired;
    public int LevelIndex;
}

[Serializable]
public class SettingsSaveData
{
    public bool HasSettings;

    public bool MusicEnabled = true;
    public bool EffectsEnabled = true;
    public float MusicVolume = 0.5f;
    public float EffectsVolume = 0.5f;
}

[Serializable]
public class QuestSaveData
{
    public bool HasQuestSave;

    // Квест, который сейчас работает и набирает прогресс.
    public int ActiveQuestIndex;
    public int CurrentProgress;

    // Оставлено для совместимости со старым сейвом.
    public bool IsCompleted;

    // Квесты, которые уже выполнены, но награда ещё не забрана.
    public List<int> PendingRewardQuestIndexes = new();
}

[Serializable]
public class TutorialSaveData
{
    public bool HasTutorialSave;
    public int Step;
    public bool BreakdownTutorialShown;
    public bool RatingTutorialShown;
    public bool HasFirstVisitorIncome;
    public bool FirstComputerCompensationGranted;
}

[Serializable]
public class PremiumLocationSaveData
{
    public bool HasPremiumLocationSave;
    public bool IsUnlocked;
    public bool HasBonusGrantState;
    public bool BonusGranted;
}

[Serializable]
public class PotionsSaveData
{
    public bool HasPotionSave;
    public long SavedAtUtcTicks;
    public List<ActivePotionSaveData> ActivePotions = new();
}

[Serializable]
public class ActivePotionSaveData
{
    public PotionType PotionType;
    public float Duration;
    public float RemainingTime;
}
