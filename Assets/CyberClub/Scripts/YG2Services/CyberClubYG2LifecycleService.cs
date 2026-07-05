using UnityEngine;
using YG;

public class CyberClubYG2LifecycleService : MonoBehaviour
{
    [Tooltip("Включай только если в настройках YG2 выключен AutoGRA.")]
    [SerializeField] private bool _callGameReadyManually;
    [SerializeField] private bool _sendGameplayStartOnStart = true;
    private bool _gameplayStarted;

    private void Start()
    {
        if (_callGameReadyManually)
            YG2.GameReadyAPI();
        if (_sendGameplayStartOnStart)
            StartGameplay();
    }

    public void StartGameplay()
    {
        if (_gameplayStarted)
            return;
        _gameplayStarted = true;
        YG2.GameplayStart();
    }

    public void StopGameplay()
    {
        if (!_gameplayStarted)
            return;
        _gameplayStarted = false;
        YG2.GameplayStop();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus) StartGameplay(); else StopGameplay();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) StopGameplay(); else StartGameplay();
    }
}