using System;
using UnityEngine;

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
        ForceUnsubscribe();

        _questData = questData;
        CurrentProgress = 0;
        IsCompleted = false;

        Subscribe();
        NotifyProgress();
    }

    public void Restore(QuestData questData, int currentProgress, bool isCompleted)
    {
        ForceUnsubscribe();

        _questData = questData;
        CurrentProgress = ClampProgress(currentProgress);
        IsCompleted = isCompleted || CurrentProgress >= _questData.TargetValue;

        if (!IsCompleted)
            Subscribe();

        NotifyProgress();

        if (IsCompleted)
            OnCompleted?.Invoke();
    }

    public void ForceUnsubscribe()
    {
        Unsubscribe();
    }

    protected void AddProgress(int amount)
    {
        if (IsCompleted || _questData == null)
            return;

        CurrentProgress = ClampProgress(CurrentProgress + amount);

        if (CurrentProgress >= _questData.TargetValue)
        {
            IsCompleted = true;
            NotifyProgress();

            ForceUnsubscribe();
            OnCompleted?.Invoke();
            return;
        }

        NotifyProgress();
    }

    private int ClampProgress(int value)
    {
        if (_questData == null)
            return 0;

        return Mathf.Clamp(value, 0, _questData.TargetValue);
    }

    private void NotifyProgress()
    {
        if (_questData == null)
            return;

        OnProgressChanged?.Invoke(CurrentProgress, _questData.TargetValue);
    }

    protected abstract void Subscribe();
    protected abstract void Unsubscribe();

    public QuestData GetData()
    {
        return _questData;
    }
}
