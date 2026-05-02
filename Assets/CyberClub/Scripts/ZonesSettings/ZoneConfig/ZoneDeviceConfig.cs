using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game/Zone Device Config")]
public class ZoneDeviceConfig : ScriptableObject
{
    [SerializeField] DeviceType _deviceType;
    [SerializeField] GameObject _devicePrefab;
    [SerializeField] private int _zoneId;

    public DeviceType DeviceType => _deviceType;
    public GameObject DevicePrefab => _devicePrefab;
    public int ZoneId => _zoneId;

}
