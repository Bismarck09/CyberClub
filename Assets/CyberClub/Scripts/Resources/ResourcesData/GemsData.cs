using UnityEngine;
using System;

public class GemsData : MonoBehaviour, IResource
{
    [SerializeField] private int _startGems;

    private int _currentGems;

    public ResourceType Type { get; set; }
    public int CurrentGems => _currentGems;

    public event Action<int> OnGemsChanged;

    private void Awake()
    {
        Type = ResourceType.Gems;
        _currentGems = _startGems;
    }

    private void Start()
    {
        OnGemsChanged?.Invoke(0);
    }

    public bool TryBuy(int amount)
    {
        if (_currentGems < amount)
            return false;

        RemoveGems(amount);
        return true;
    }

    public void AddResource(int amount, float multiplier)
    {
        if (amount == 0)
            return;

        int finalAmount = Mathf.RoundToInt(amount * multiplier);
        _currentGems += finalAmount;
        OnGemsChanged?.Invoke(finalAmount);
    }

    public void SetGems(int value)
    {
        _currentGems = Mathf.Max(0, value);
        OnGemsChanged?.Invoke(0);
    }

    private void RemoveGems(int amount)
    {
        _currentGems = Mathf.Max(0, _currentGems - amount);
        OnGemsChanged?.Invoke(-amount);
    }
}

