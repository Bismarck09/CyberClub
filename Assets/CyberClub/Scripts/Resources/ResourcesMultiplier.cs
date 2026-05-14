using UnityEngine;
using System.Collections.Generic;

public class ResourcesMultiplier : MonoBehaviour
{
    private Dictionary<ResourceType, int> _multipliers = new();

    private void Awake()
    {
        _multipliers[ResourceType.Coins] = 1;
        _multipliers[ResourceType.Gems] = 1;
    }

    public int GetMultiplier(ResourceType type)
    {
        return _multipliers[type];
    }

    public void SetMultiplier(ResourceType type, int value)
    {
        _multipliers[type] = value;
    }

    public void ResetMultiplier(ResourceType type)
    {
        _multipliers[type] = 1;
    }
}
