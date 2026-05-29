using UnityEngine;
using System;

public class CoinsData : MonoBehaviour, IResource
{
    private int _currentCoins;

    public ResourceType Type {get; set;}
    public int CurrentCoins => _currentCoins;
    public event Action<int> OnCoinsChanged;

    void Start()
    {
        Type = ResourceType.Coins;
        AddResource(10000, 1);
    }

    public bool TryBuy(int amount)
    {
        if (_currentCoins >= amount)
        {
            RemoveCoins(amount);
            return true;
        }
        return false;
    }

    public void AddResource(int amount, float multiplier)
    {
        if (amount == 0)
            return;

        _currentCoins += Mathf.RoundToInt(amount * multiplier);
        OnCoinsChanged?.Invoke(Mathf.RoundToInt(amount * multiplier));
    }

    private void RemoveCoins(int amount)
    {
        _currentCoins -= amount;

        OnCoinsChanged?.Invoke(-amount);
    }
}
