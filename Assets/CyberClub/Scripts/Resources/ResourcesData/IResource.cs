using UnityEngine;

public interface IResource
{
    ResourceType Type {get; set;}
    bool TryBuy(int amount);
    void AddResource(int amount, int multiplier);
}
