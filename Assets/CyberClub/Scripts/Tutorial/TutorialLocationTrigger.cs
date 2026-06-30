using UnityEngine;

public class TutorialLocationTrigger : MonoBehaviour
{
    [SerializeField] private CyberClubTutorialManager _tutorialManager;
    [SerializeField] private TutorialTriggerType _triggerType = TutorialTriggerType.FirstRoom;
    [SerializeField] private string _playerTag = "Player";
    [SerializeField] private bool _disableAfterTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(_playerTag))
            return;

        if (_tutorialManager == null)
            _tutorialManager = FindFirstObjectByType<CyberClubTutorialManager>();

        if (_tutorialManager == null)
            return;

        switch (_triggerType)
        {
            case TutorialTriggerType.FirstRoom:
                _tutorialManager.OnPlayerEnteredFirstRoom();
                break;
        }

        if (_disableAfterTrigger)
            gameObject.SetActive(false);
    }
}

public enum TutorialTriggerType
{
    FirstRoom
}