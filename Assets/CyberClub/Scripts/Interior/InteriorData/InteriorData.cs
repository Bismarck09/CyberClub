using System.Collections.Generic;
using UnityEngine;

public class InteriorData : MonoBehaviour
{
    [SerializeField] private List<GameObject> _interiorObjects;
    [SerializeField] private List<int> _interiorsPrice;
    [SerializeField] private List<float> _multipliers;

    private int _currentBoughtInteriorObjects;

    public float GetCoinsMultiplier()
    {
        float multiplier = 1;

        for (int i = 0; i < _currentBoughtInteriorObjects; i++)
        {
            multiplier += _multipliers[i];
        }
        
        return multiplier;
    }
}