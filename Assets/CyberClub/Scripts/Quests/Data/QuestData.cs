using UnityEngine;

[CreateAssetMenu(fileName = "QuestData", menuName = "Scriptable Objects/QuestData")]
public class QuestData : ScriptableObject
{
    [SerializeField] private QuestType _questType;
    [SerializeField] private int _targetValue;
    [SerializeField] private int _rewardValue;
    
    public QuestType Type => _questType;
    public int TargetValue => _targetValue;
    public int RewardValue => _rewardValue;
}
