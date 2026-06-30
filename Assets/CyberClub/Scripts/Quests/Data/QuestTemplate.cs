using System;

[Serializable]
public class QuestTemplate
{
    public QuestType Type;
    public int Target;
    public int Reward;
    public string Description;

    public QuestTemplate(QuestType type, int target, int reward, string description)
    {
        Type = type;
        Target = target;
        Reward = reward;
        Description = description;
    }
}