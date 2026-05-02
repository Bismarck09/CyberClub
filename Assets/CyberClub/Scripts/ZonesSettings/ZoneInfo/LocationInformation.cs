using UnityEngine;

public class LocationInformation : MonoBehaviour
{
    [SerializeField] private ZoneSwitcher _zoneSwitcher;

    private bool _isDeviceRemained;

    public ZoneDeviceConfig ZoneConfig {get; private set;}
    public SpawnPointsHolder SpawnPoints {get; private set;}

    public bool IsDeviceRemained => _isDeviceRemained;

    void OnEnable()
    {
        _zoneSwitcher.OnZoneChanged += ChangeZone;
    }

    void OnDisable()
    {
        _zoneSwitcher.OnZoneChanged -= ChangeZone;
    }


    private void ChangeZone(ZoneInformation newZoneInformation)
    {
        ZoneConfig = newZoneInformation.ZoneConfig;
        SpawnPoints = newZoneInformation.SpawnPoints;

        if (SpawnPoints != null)
            _isDeviceRemained = true;
        else
            _isDeviceRemained = false;

    }
}
