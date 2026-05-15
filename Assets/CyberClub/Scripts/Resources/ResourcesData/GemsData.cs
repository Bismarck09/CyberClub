using UnityEngine;
using System;

public class GemsData : MonoBehaviour, IResource
{
    private int _currentGems;

    public ResourceType Type {get; set;}
    public int CurrentGems => _currentGems;
    public event Action<int> OnGemsChanged;

    void Start()
    {
        Type = ResourceType.Gems;
    }

    public bool TryBuy(int amount)
    {
        if (_currentGems >= amount)
        {
            RemoveGems(amount);
            return true;
        }
        return false;
    }

    public void AddResource(int amount, float multiplier)
    {
        if (amount == 0)
            return;

        _currentGems += Mathf.RoundToInt(amount * multiplier);

        OnGemsChanged?.Invoke(Mathf.RoundToInt(amount * multiplier));
    }

    private void RemoveGems(int amount)
    {
        _currentGems -= amount;

        OnGemsChanged?.Invoke(-amount);
    }
}
