using System;
using UnityEngine;

public class ZoneSwitcher : MonoBehaviour
{
    public event Action<ZoneInformation> OnZoneChanged;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out ZoneInformation zoneInformation))
            OnZoneChanged?.Invoke(zoneInformation);

        Debug.Log("Zone switched to " + zoneInformation.name);
    }
}
