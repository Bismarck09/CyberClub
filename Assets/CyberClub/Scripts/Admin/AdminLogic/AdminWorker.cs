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
    public int QueueCount => _queue.Count;

    public event Action OnChanged;

    private void Awake()
    {
        if (_isHiredOnStart)
            Hire();
        else
            SetQueuePointsActive(false);
    }

    public bool HasFreeQueueSlot()
    {
        return _isHired && _queue.Count < _queuePoints.Count;
    }

    public Transform AddVisitorToQueue(Visitor visitor)
    {
        if (!HasFreeQueueSlot())
            return null;

        _queue.Add(visitor);
        return _queuePoints[_queue.Count - 1];
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

        VisitorRegistration registration = visitor.GetComponent<VisitorRegistration>();

        if (registration == null || registration.IsRegistered == false)
            return null;

        return visitor;
    }

    public void RemoveVisitor(Visitor visitor)
    {
        if (_queue.Contains(visitor))
        {
            _queue.Remove(visitor);
            MoveQueue();
            OnChanged?.Invoke();
        }
    }

    public void Hire()
    {
        _isHired = true;
        _levelIndex = 0;
        gameObject.SetActive(true);

        SetQueuePointsActive(true);
        OnChanged?.Invoke();
    }

    public bool CanUpgrade()
    {
        return _levelIndex + 1 < _levels.Count;
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
        OnChanged?.Invoke();
    }

    public float GetServiceInterval()
    {
        if (_levels.Count == 0)
            return 10f;

        return _levels[_levelIndex].ServiceInterval;
    }

    public void SetBusy(bool value)
    {
        _isBusy = value;
        OnChanged?.Invoke();
    }

    private void MoveQueue()
    {
        for (int i = 0; i < _queue.Count; i++)
        {
            if (_queue[i] == null)
                continue;

            VisitorRegistration registration = _queue[i].GetComponent<VisitorRegistration>();

            if (registration == null || registration.IsRegistered == false)
                continue;

            VisitorMovement movement = _queue[i].GetComponent<VisitorMovement>();

            if (movement != null)
                movement.Move(_queuePoints[i].position);
        }
    }

    private void SetQueuePointsActive(bool value)
    {
        foreach (Transform point in _queuePoints)
        {
            if (point != null)
                point.gameObject.SetActive(value);
        }
    }
}