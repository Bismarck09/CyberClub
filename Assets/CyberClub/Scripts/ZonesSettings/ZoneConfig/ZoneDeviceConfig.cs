using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game/Zone Device Config")]
public class ZoneDeviceConfig : ScriptableObject
{
    [SerializeField] DeviceType _deviceType;
    [SerializeField] GameObject _devicePrefab;
    [SerializeField] private int _priceOfHourCoins;
    [SerializeField] private int _priceOfHourGems; 
    [SerializeField] private int _devicePrice;

    public DeviceType DeviceType => _deviceType;
    public GameObject DevicePrefab => _devicePrefab;
    public int PriceOfHourCoins => _priceOfHourCoins;
    public int PriceOfHourGems => _priceOfHourGems;
    public int DevicePrice => _devicePrice;
    
    public void IncreaseDevicePrice()
    {
        _devicePrice += Mathf.RoundToInt(_devicePrice * 0.1f);
    }

}
