using UnityEngine;
using YG;

public class CyberClubYG2PauseBridge : MonoBehaviour
{
    [SerializeField] private TutorialInputBlocker _inputBlocker;

    private void OnEnable() => YG2.onPauseGame += OnYG2PauseChanged;
    private void OnDisable() => YG2.onPauseGame -= OnYG2PauseChanged;

    private void OnYG2PauseChanged(bool isPaused)
    {
        if (_inputBlocker != null)
            _inputBlocker.SetBlocked(isPaused);
    }
}