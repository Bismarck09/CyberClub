using System;

public abstract class Quest
{
    protected QuestData _questData;

    public IResource ResourceData { get; private set; }
    public int CurrentProgress { get; private set; }
    public bool IsCompleted { get; private set; }
    public QuestType Type { get; private set; }

    public event Action<int, int> OnProgressChanged;
    public event Action OnCompleted;

    public Quest(QuestType type, IResource resourceData)
    {
        Type = type;
        ResourceData = resourceData;
    }

    public void Activate(QuestData questData)
    {
        _questData = questData;
        CurrentProgress = 0;
        IsCompleted = false;

        Subscribe();
        OnProgressChanged?.Invoke(CurrentProgress, _questData.TargetValue);
    }

    protected void AddProgress(int amount)
    {
        if (IsCompleted)
            return;

        CurrentProgress += amount;

        if (CurrentProgress >= _questData.TargetValue)
        {
            CurrentProgress = _questData.TargetValue;
            IsCompleted = true;

            OnProgressChanged?.Invoke(CurrentProgress, _questData.TargetValue);

            Unsubscribe();
            OnCompleted?.Invoke();

            return;
        }

        OnProgressChanged?.Invoke(CurrentProgress, _questData.TargetValue);
    }

    protected abstract void Subscribe();
    protected abstract void Unsubscribe();

    public QuestData GetData()
    {
        return _questData;
    }
}
