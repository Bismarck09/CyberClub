using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game/Zone Device Config")]
public class ZoneDeviceConfig : ScriptableObject
{
    [SerializeField] DeviceType _deviceType;
    [SerializeField] GameObject _devicePrefab;
    [SerializeField] private float _priceOfHourCoins;
    [SerializeField] private float _priceOfHourGems; 

    public DeviceType DeviceType => _deviceType;
    public GameObject DevicePrefab => _devicePrefab;
    public float PriceOfHourCoins => _priceOfHourCoins;
    public float PriceOfHourGems => _priceOfHourGems;

}
