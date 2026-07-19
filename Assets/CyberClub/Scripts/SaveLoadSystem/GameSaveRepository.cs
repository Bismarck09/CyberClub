using UnityEngine;

public static class GameSaveRepository
{
    private const string SaveKey = "CyberClub_GameSave_v1";

    public static bool HasSave => PlayerPrefs.HasKey(SaveKey);

    public static void Save(GameSaveData saveData)
    {
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public static GameSaveData Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
            return new GameSaveData();

        string json = PlayerPrefs.GetString(SaveKey);

        if (string.IsNullOrWhiteSpace(json))
            return new GameSaveData();

        try
        {
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json) ?? new GameSaveData();

            // ИЗМЕНЕНО: новые разделы и коллекции создаются для старых JSON,
            // где этих полей ещё не существовало.
            data.Resources ??= new ResourceSaveData();
            data.Rating ??= new RatingSaveData();
            data.Zones ??= new System.Collections.Generic.List<ZoneSaveData>();
            data.Admins ??= new System.Collections.Generic.List<AdminSaveData>();
            data.Settings ??= new SettingsSaveData();
            data.Quests ??= new QuestSaveData();
            data.Tutorial ??= new TutorialSaveData();
            data.PremiumLocation ??= new PremiumLocationSaveData();
            data.Potions ??= new PotionsSaveData();
            data.Potions.ActivePotions ??= new System.Collections.Generic.List<ActivePotionSaveData>();
            return data;
        }
        catch
        {
            Debug.LogError("GameSaveRepository: сохранение повреждено, создан новый GameSaveData.");
            return new GameSaveData();
        }
    }

    public static void Delete()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }
}
