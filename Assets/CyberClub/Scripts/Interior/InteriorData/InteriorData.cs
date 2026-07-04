using System.Collections.Generic;
using UnityEngine;

public class InteriorData : MonoBehaviour
{
    [SerializeField] private List<GameObject> _interiorObjects;
    [SerializeField] private List<int> _interiorsPrice;
    [SerializeField] private List<float> _multipliers;

    private int _currentBoughtInteriorObjects;

    public int CurrentBoughtInteriorObjects => _currentBoughtInteriorObjects;
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

    private void Awake()
    {
        RefreshVisuals();
    }

    public float GetCoinsMultiplier()
    {
        float multiplier = 0f;
        int count = Mathf.Min(_currentBoughtInteriorObjects, _multipliers.Count);

        for (int i = 0; i < count; i++)
            multiplier += _multipliers[i];

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

        _currentBoughtInteriorObjects++;
        RefreshVisuals();
    }

    public void RestoreBoughtInteriorObjects(int count)
    {
        _currentBoughtInteriorObjects = Mathf.Clamp(count, 0, _interiorObjects.Count);
        RefreshVisuals();
    }

    private void RefreshVisuals()
    {
        for (int i = 0; i < _interiorObjects.Count; i++)
        {
            if (_interiorObjects[i] != null)
                _interiorObjects[i].SetActive(i < _currentBoughtInteriorObjects);
        }
    }
}
