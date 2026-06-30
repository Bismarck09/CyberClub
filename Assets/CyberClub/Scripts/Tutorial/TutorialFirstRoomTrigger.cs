using UnityEngine;

public class TutorialFirstRoomTrigger : MonoBehaviour
{
    [SerializeField] private CyberClubTutorialManager _tutorialManager;
    [SerializeField] private string _playerTag = "Player";
    [SerializeField] private bool _disableAfterTrigger = true;

    private void Awake()
    {
        if (_tutorialManager == null)
            _tutorialManager = FindFirstObjectByType<CyberClubTutorialManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(_playerTag))
            return;

        if (_tutorialManager != null)
            _tutorialManager.EnterFirstRoom();

        if (_disableAfterTrigger)
            gameObject.SetActive(false);
    }
}