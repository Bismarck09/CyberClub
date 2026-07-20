using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Zone Device Config")]
public class ZoneDeviceConfig : ScriptableObject
{
    [SerializeField] private DeviceType _deviceType;
    [SerializeField] private GameObject _devicePrefab;
    [SerializeField] private bool _isPremiumZone;

    [Header("Income")]
    [SerializeField] private int _priceOfHourCoins;
    [SerializeField] private int _priceOfHourGems;

    [Header("Purchase")]
    [SerializeField] private int _devicePrice;
    [SerializeField] private float _priceGrowthPercent = 10f;

    public DeviceType DeviceType => _deviceType;
    public GameObject DevicePrefab => _devicePrefab;
    public bool IsPremiumZone => _isPremiumZone;
    public int PriceOfHourCoins => _priceOfHourCoins;
    public int PriceOfHourGems => _priceOfHourGems;
    public int DevicePrice => _devicePrice;
    public float PriceGrowthPercent => _priceGrowthPercent;

    public int CalculateDevicePrice(int purchasedDeviceCount)
    {
        int price = Mathf.Max(0, _devicePrice);
        int count = Mathf.Max(0, purchasedDeviceCount);
        float growth = Mathf.Max(0f, _priceGrowthPercent) / 100f;

        for (int i = 0; i < count; i++)
            price += Mathf.RoundToInt(price * growth);

        return price;
    }

    [Obsolete("Runtime-цена рассчитывается через CalculateDevicePrice и количество покупок зоны.")]
    public void IncreaseDevicePrice()
    {
        Debug.LogWarning(
            $"{name}: IncreaseDevicePrice больше не изменяет ScriptableObject. " +
            "Используйте CalculateDevicePrice с фактическим количеством покупок.",
            this);
    }
}
