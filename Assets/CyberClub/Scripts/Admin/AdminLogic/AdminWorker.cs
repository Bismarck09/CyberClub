using System;
using System.Collections.Generic;
using UnityEngine;

public class AdminWorker : MonoBehaviour
{
    [SerializeField] private string _displayName = "Админ";
    [SerializeField] private int _hirePrice = 1000;
    [SerializeField] private bool _isHiredOnStart;
    [SerializeField] private List<Transform> _queuePoints;
    [SerializeField] private List<AdminLevelData> _levels;

    private readonly List<Visitor> _queue = new();

    private int _levelIndex;
    private bool _isHired;
    private bool _isBusy;

    public string DisplayName => _displayName;
    public int HirePrice => _hirePrice;
    public bool IsHired => _isHired;
    public bool IsBusy => _isBusy;
    public int Level => _levelIndex + 1;
    public int LevelIndex => _levelIndex;
    public int QueueCount => _queue.Count;

    // ИЗМЕНЕНО: точное количество свободных мест.
    public int FreeQueueSlotCount
    {
        get
        {
            if (!_isHired || _queuePoints == null)
                return 0;

            return Mathf.Max(0, _queuePoints.Count - _queue.Count);
        }
    }

    public event Action<AdminWorker> OnChanged;

    private void Awake()
    {
        RestoreState(_isHiredOnStart, 0);
    }

    public bool HasFreeQueueSlot()
    {
        return FreeQueueSlotCount > 0;
    }

    public Transform AddVisitorToQueue(Visitor visitor)
    {
        // ИЗМЕНЕНО: защита от null и повторного добавления.
        if (visitor == null || !HasFreeQueueSlot())
            return null;

        if (_queue.Contains(visitor))
            return null;

        _queue.Add(visitor);

        int pointIndex = _queue.Count - 1;

        if (pointIndex < 0 || pointIndex >= _queuePoints.Count)
        {
            _queue.Remove(visitor);
            return null;
        }

        return _queuePoints[pointIndex];
    }

    public Visitor GetNextVisitor()
    {
        if (!_isHired || _isBusy || _queue.Count == 0)
            return null;

        Visitor visitor = _queue[0];

        if (visitor == null)
        {
            _queue.RemoveAt(0);
            MoveQueue();
            return null;
        }

        VisitorRegistration registration =
            visitor.GetComponent<VisitorRegistration>();

        if (registration == null || !registration.IsRegistered)
            return null;

        return visitor;
    }

    public void RemoveVisitor(Visitor visitor)
    {
        if (visitor == null || !_queue.Contains(visitor))
            return;

        _queue.Remove(visitor);
        MoveQueue();
        OnChanged?.Invoke(this);
    }

    public void Hire()
    {
        RestoreState(true, 0);
    }

    public bool CanUpgrade()
    {
        return _levels != null && _levelIndex + 1 < _levels.Count;
    }

    public int GetUpgradePrice()
    {
        if (!CanUpgrade())
            return 0;

        return _levels[_levelIndex + 1].UpgradePrice;
    }

    public void Upgrade()
    {
        if (!CanUpgrade())
            return;

        _levelIndex++;
        OnChanged?.Invoke(this);
    }

    public float GetServiceInterval()
    {
        if (_levels == null || _levels.Count == 0)
            return 10f;

        return _levels[_levelIndex].ServiceInterval;
    }

    public void SetBusy(bool value)
    {
        _isBusy = value;
        OnChanged?.Invoke(this);
    }

    public void RestoreState(bool isHired, int levelIndex)
    {
        _queue.Clear();
        _isBusy = false;
        _isHired = isHired;

        int maxLevelIndex =
            _levels != null
                ? Mathf.Max(0, _levels.Count - 1)
                : 0;

        _levelIndex = Mathf.Clamp(levelIndex, 0, maxLevelIndex);

        gameObject.SetActive(_isHired);
        SetQueuePointsActive(_isHired);
        OnChanged?.Invoke(this);
    }

    private void MoveQueue()
    {
        if (_queuePoints == null)
            return;

        for (int i = 0; i < _queue.Count; i++)
        {
            if (i >= _queuePoints.Count)
                break;

            Visitor visitor = _queue[i];

            if (visitor == null)
                continue;

            VisitorRegistration registration =
                visitor.GetComponent<VisitorRegistration>();

            if (registration == null || !registration.IsRegistered)
                continue;

            VisitorMovement movement =
                visitor.GetComponent<VisitorMovement>();

            if (movement != null && _queuePoints[i] != null)
                movement.Move(_queuePoints[i].position);
        }
    }

    private void SetQueuePointsActive(bool value)
    {
        if (_queuePoints == null)
            return;

        foreach (Transform point in _queuePoints)
        {
            if (point != null)
                point.gameObject.SetActive(value);
        }
    }
}