using UnityEngine;

public class TutorialBreakdownHook : MonoBehaviour
{
    [SerializeField] private CyberClubTutorialManager _tutorialManager;

    private void Awake()
    {
        if (_tutorialManager == null)
            _tutorialManager = FindFirstObjectByType<CyberClubTutorialManager>();
    }

    public void ShowBreakdownTutorial()
    {
        if (_tutorialManager != null)
            _tutorialManager.OnBreakdownStartedManually();
    }
}