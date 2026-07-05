using UnityEngine;

public class TutorialSaveModule : MonoBehaviour, ISaveModule
{
    [SerializeField] private CyberClubTutorialManager _tutorialManager;

    public void Capture(GameSaveData saveData)
    {
        if (_tutorialManager == null)
            return;

        saveData.Tutorial = _tutorialManager.CaptureSave();
    }

    public void Restore(GameSaveData saveData)
    {
        if (_tutorialManager == null || saveData.Tutorial == null || saveData.Tutorial.HasTutorialSave == false)
            return;

        _tutorialManager.RestoreSave(saveData.Tutorial);
    }
}