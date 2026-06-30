using UnityEngine;

[CreateAssetMenu(fileName = "QuestData", menuName = "Scriptable Objects/QuestData")]
public class QuestData : ScriptableObject
{
    [SerializeField] private QuestType _questType;
    [SerializeField] private int _targetValue;
    [SerializeField] private int _rewardValue;
    [SerializeField] private string _description;

    public QuestType Type => _questType;
    public int TargetValue => _targetValue;
    public int RewardValue => _rewardValue;

    public string Description
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_description))
                return _description;

            return _questType switch
            {
                QuestType.BuyDevice => $"Купи {_targetValue} компьютера",
                QuestType.VisitorService => $"Обслужи {_targetValue} клиентов",
                _ => "Выполни задание"
            };
        }
    }

    public void InitRuntime(QuestType questType, int targetValue, int rewardValue, string description)
    {
        _questType = questType;
        _targetValue = Mathf.Max(1, targetValue);
        _rewardValue = Mathf.Max(0, rewardValue);
        _description = description;
    }

    public static QuestData CreateRuntime(QuestType questType, int targetValue, int rewardValue, string description)
    {
        QuestData data = CreateInstance<QuestData>();
        data.InitRuntime(questType, targetValue, rewardValue, description);
        return data;
    }
}