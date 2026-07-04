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
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
            return data ?? new GameSaveData();
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