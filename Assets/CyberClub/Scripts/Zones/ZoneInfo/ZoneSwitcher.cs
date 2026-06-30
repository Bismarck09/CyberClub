using System;
using UnityEngine;

public class ZoneSwitcher : MonoBehaviour
{
    public event Action<ZoneInformation> OnZoneChanged;
    public event Action OnZoneExited;

    private ZoneInformation _currentZone;

    public ZoneInformation CurrentZone => _currentZone;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.TryGetComponent(out ZoneInformation zoneInformation))
            return;

        _currentZone = zoneInformation;
        OnZoneChanged?.Invoke(zoneInformation);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.TryGetComponent(out ZoneInformation zoneInformation))
            return;

        if (_currentZone == zoneInformation)
            _currentZone = null;

        OnZoneExited?.Invoke();
    }
}