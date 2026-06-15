using UnityEngine;

[CreateAssetMenu(fileName = "QuestData", menuName = "Scriptable Objects/QuestData")]
public class QuestData : ScriptableObject
{
    [SerializeField] private QuestType _questType;
    [SerializeField] private string _descriptionTemplate;
    [SerializeField] private int _targetValue;
    [SerializeField] private int _rewardValue;

    public QuestType Type => _questType;
    public int TargetValue => _targetValue;
    public int RewardValue => _rewardValue;

    public string GetDescription()
    {
        if (string.IsNullOrWhiteSpace(_descriptionTemplate))
            return GetDefaultDescription();

        return string.Format(_descriptionTemplate, _targetValue);
    }

    private string GetDefaultDescription()
    {
        switch (_questType)
        {
            case QuestType.VisitorService:
                return $"Обслужи {_targetValue} клиентов";

            case QuestType.BuyDevice:
                return $"Купи {_targetValue} компьютеров";

            default:
                return $"Выполни действие {_targetValue} раз";
        }
    }
}