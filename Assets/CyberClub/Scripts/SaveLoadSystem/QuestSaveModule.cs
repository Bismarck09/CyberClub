using UnityEngine;

public class QuestsSaveModule : MonoBehaviour, ISaveModule
{
    [SerializeField] private QuestCreator _questCreator;

    public void Capture(GameSaveData saveData)
    {
        if (_questCreator == null)
            return;

        saveData.Quests = _questCreator.CaptureSave();
    }

    public void Restore(GameSaveData saveData)
    {
        if (_questCreator == null || saveData.Quests == null || saveData.Quests.HasQuestSave == false)
            return;

        _questCreator.RestoreSave(saveData.Quests);
    }
}