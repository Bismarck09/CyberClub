using System.Collections.Generic;
using UnityEngine;

public class InteriorData : MonoBehaviour
{
    [SerializeField] private List<GameObject> _interiorObjects;
    [SerializeField] private List<int> _interiorsPrice;
    [SerializeField] private List<float> _multipliers;

    private int _currentBoughtInteriorObjects;

    public bool IsMaxPurchased => _currentBoughtInteriorObjects >= _interiorObjects.Count;

    public int InteriorsPrice
    {
        get
        {
            if (IsMaxPurchased)
                return 0;

            if (_currentBoughtInteriorObjects >= _interiorsPrice.Count)
            {
                Debug.LogError("Не хватает цены для следующего интерьера");
                return 0;
            }

            return _interiorsPrice[_currentBoughtInteriorObjects];
        }
    }

    public float GetCoinsMultiplier()
    {
        float multiplier = 0f;

        int count = Mathf.Min(_currentBoughtInteriorObjects, _multipliers.Count);

        for (int i = 0; i < count; i++)
        {
            multiplier += _multipliers[i];
        }

        return multiplier;
    }

    public void BuyInterior()
    {
        if (IsMaxPurchased)
            return;

        if (_currentBoughtInteriorObjects >= _interiorObjects.Count)
        {
            Debug.LogError("Не хватает объекта интерьера");
            return;
        }

        if (_currentBoughtInteriorObjects >= _interiorsPrice.Count)
        {
            Debug.LogError("Не хватает цены интерьера");
            return;
        }

        if (_currentBoughtInteriorObjects >= _multipliers.Count)
        {
            Debug.LogError("Не хватает множителя интерьера");
            return;
        }

        GameObject interiorObject = _interiorObjects[_currentBoughtInteriorObjects];

        if (interiorObject != null)
        {
            interiorObject.SetActive(true);
        }

        _currentBoughtInteriorObjects++;
    }
}