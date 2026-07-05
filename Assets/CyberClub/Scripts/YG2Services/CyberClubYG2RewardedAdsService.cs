using System;
using UnityEngine;
using YG;

public class CyberClubYG2RewardedAdsService : MonoBehaviour
{
    [Header("Reward IDs")]
    [SerializeField] private string _coinsRewardId = "reward_coins";
    [SerializeField] private string _gemsRewardId = "reward_gems";

    [Header("Rewards")]
    [SerializeField] private int _coinsReward = 5000;
    [SerializeField] private int _gemsReward = 50;

    [Header("Data")]
    [SerializeField] private CoinsData _coinsData;
    [SerializeField] private GemsData _gemsData;
    [SerializeField] private SaveLoadManager _saveLoadManager;

    private bool _isRewardAdOpening;

    public event Action<string> OnRewardStarted;
    public event Action<string> OnRewardReceived;
    public event Action<string> OnRewardFailed;

    private void OnEnable()
    {
        YG2.onErrorRewardedAdv += OnRewardedAdError;
        YG2.onCloseRewardedAdv += OnRewardedAdClosed;
    }

    private void OnDisable()
    {
        YG2.onErrorRewardedAdv -= OnRewardedAdError;
        YG2.onCloseRewardedAdv -= OnRewardedAdClosed;
    }

    public void ShowCoinsRewardAd() => ShowRewardedAd(_coinsRewardId, GiveCoinsReward);
    public void ShowGemsRewardAd() => ShowRewardedAd(_gemsRewardId, GiveGemsReward);

    private void ShowRewardedAd(string rewardId, Action rewardCallback)
    {
        if (_isRewardAdOpening)
            return;

        _isRewardAdOpening = true;
        OnRewardStarted?.Invoke(rewardId);

        YG2.RewardedAdvShow(rewardId, () =>
        {
            _isRewardAdOpening = false;
            rewardCallback?.Invoke();
            OnRewardReceived?.Invoke(rewardId);

            if (_saveLoadManager != null)
                _saveLoadManager.SaveGame();
        });
    }

    private void GiveCoinsReward()
    {
        if (_coinsData == null)
        {
            Debug.LogError("CoinsData не назначен.");
            return;
        }
        _coinsData.AddResource(_coinsReward, 1f);
    }

    private void GiveGemsReward()
    {
        if (_gemsData == null)
        {
            Debug.LogError("GemsData не назначен.");
            return;
        }
        _gemsData.AddResource(_gemsReward, 1f);
    }

    private void OnRewardedAdError()
    {
        _isRewardAdOpening = false;
        OnRewardFailed?.Invoke("rewarded_error");
    }

    private void OnRewardedAdClosed() => _isRewardAdOpening = false;
}
