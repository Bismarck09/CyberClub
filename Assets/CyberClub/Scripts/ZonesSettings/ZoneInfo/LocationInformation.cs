using UnityEngine;

public class LocationInformation : MonoBehaviour
{
    [SerializeField] private ZoneSwitcher _zoneSwitcher;
    [SerializeField] private InteriorPurchase _interiorPurchase;

    private bool _isDeviceRemained;

    public ZoneDeviceConfig ZoneConfig {get; private set;}
    public SpawnPointsHolder SpawnPoints {get; private set;}
    public float Multiplier {get; private set;}

    public bool IsDeviceRemained => _isDeviceRemained;

    void OnEnable()
    {
        _zoneSwitcher.OnZoneChanged += ChangeZone;
        _interiorPurchase.OnInteriorPurchase += ChangeMultiplier;
    }

    void OnDisable()
    {
        _zoneSwitcher.OnZoneChanged -= ChangeZone;
        _interiorPurchase.OnInteriorPurchase -= ChangeMultiplier;
    }

    private void ChangeMultiplier(InteriorData interiorData)
    {
        Multiplier = interiorData.GetCoinsMultiplier();
    }

    private void ChangeZone(ZoneInformation newZoneInformation)
    {
        ZoneConfig = newZoneInformation.ZoneConfig;
        SpawnPoints = newZoneInformation.SpawnPoints;
        Multiplier = newZoneInformation.GetCoinsMultiplier();

        if (SpawnPoints != null)
            _isDeviceRemained = true;
        else
            _isDeviceRemained = false;

    }
}
