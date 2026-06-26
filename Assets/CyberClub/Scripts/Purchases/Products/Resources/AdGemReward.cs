using UnityEngine;

public class AdGemReward : MonoBehaviour
{
    [SerializeField] private GemsData _gemsData;
    [SerializeField] private int _rewardGems = 20;

    public void GiveRewardForWatchedAd()
    {
        if (_gemsData == null)
        {
            Debug.LogError("AdGemReward: не назначен GemsData.");
            return;
        }

        _gemsData.AddResource(_rewardGems, 1f);
    }
}