using System;
using UnityEngine;

public class ZoneSwitcher : MonoBehaviour
{
    public event Action<ZoneInformation> OnZoneChanged;
    public event Action OnZoneExited;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out ZoneInformation zoneInformation))
            OnZoneChanged?.Invoke(zoneInformation);

        Debug.Log("Zone switched to " + zoneInformation.name);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out ZoneInformation zoneInformation))
            OnZoneExited?.Invoke();
    }
}
