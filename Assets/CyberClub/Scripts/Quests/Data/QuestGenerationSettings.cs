using UnityEngine;

[CreateAssetMenu(fileName = "QuestGenerationSettings", menuName = "CyberClub/Quests/Quest Generation Settings")]
public class QuestGenerationSettings : ScriptableObject
{
    [Header("Amount")]
    [SerializeField] private int _questCount = 100;

    [Header("Visitor quests")]
    [SerializeField] private int _startVisitorTarget = 10;
    [SerializeField] private int _visitorTargetStep = 5;

    [Header("Device quests")]
    [SerializeField] private int _minDeviceTarget = 2;
    [SerializeField] private int _maxDeviceTarget = 4;
    [SerializeField] private int _randomSeed = 12345;

    [Header("Rewards")]
    [SerializeField] private int _startReward = 1600;
    [SerializeField] private float _rewardGrowth = 1.08f;
    [SerializeField] private int _rewardFlatBonus = 350;

    public int QuestCount => Mathf.Max(1, _questCount);
    public int StartVisitorTarget => Mathf.Max(1, _startVisitorTarget);
    public int VisitorTargetStep => Mathf.Max(1, _visitorTargetStep);
    public int MinDeviceTarget => Mathf.Max(1, _minDeviceTarget);
    public int MaxDeviceTarget => Mathf.Max(MinDeviceTarget, _maxDeviceTarget);
    public int RandomSeed => _randomSeed;
    public int StartReward => Mathf.Max(0, _startReward);
    public float RewardGrowth => Mathf.Max(1f, _rewardGrowth);
    public int RewardFlatBonus => Mathf.Max(0, _rewardFlatBonus);
}