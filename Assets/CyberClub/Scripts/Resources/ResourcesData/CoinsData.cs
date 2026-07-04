using UnityEngine;
using System;

public class CoinsData : MonoBehaviour, IResource
{
    [SerializeField] private int _startCoins = 1000;

    private int _currentCoins;

    public ResourceType Type { get; set; }
    public int CurrentCoins => _currentCoins;

    public event Action<int> OnCoinsChanged;

    private void Awake()
    {
        Type = ResourceType.Coins;
        _currentCoins = _startCoins;
    }

    private void Start()
    {
        OnCoinsChanged?.Invoke(0);
    }

    public bool TryBuy(int amount)
    {
        if (_currentCoins < amount)
            return false;

        RemoveCoins(amount);
        return true;
    }

    public void AddResource(int amount, float multiplier)
    {
        if (amount == 0)
            return;

        int finalAmount = Mathf.RoundToInt(amount * multiplier);
        _currentCoins += finalAmount;
        OnCoinsChanged?.Invoke(finalAmount);
    }

    public void SetCoins(int value)
    {
        _currentCoins = Mathf.Max(0, value);
        OnCoinsChanged?.Invoke(0);
    }

    private void RemoveCoins(int amount)
    {
        _currentCoins = Mathf.Max(0, _currentCoins - amount);
        OnCoinsChanged?.Invoke(-amount);
    }
}