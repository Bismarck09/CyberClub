using System.Collections.Generic;
using UnityEngine;

public class InteriorData : MonoBehaviour
{
    [SerializeField] private List<GameObject> _interiorObjects;
    [SerializeField] private List<int> _interiorsPrice;
    [SerializeField] private List<float> _multipliers;

    private int _currentBoughtInteriorObjects;
    
    public int InteriorsPrice => _interiorsPrice[_currentBoughtInteriorObjects];
    public float Multiplier => _multipliers[_currentBoughtInteriorObjects];

    public float GetCoinsMultiplier()
    {
        float multiplier = 0;

        for (int i = 0; i < _currentBoughtInteriorObjects; i++)
        {
            multiplier += _multipliers[i];
        }
        
        return multiplier;
    }

    public void BuyInterior()
    {
        _interiorObjects[_currentBoughtInteriorObjects].SetActive(true);
        _currentBoughtInteriorObjects++;
    }
}