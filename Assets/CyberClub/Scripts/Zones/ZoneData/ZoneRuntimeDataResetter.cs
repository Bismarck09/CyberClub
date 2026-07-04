using UnityEngine;

public class ZoneRuntimeDataResetter : MonoBehaviour
{
    [SerializeField] private ZoneInformation[] _zones;

    [ContextMenu("Reset All Zone Runtime Data")]
    public void ResetAll()
    {
        foreach (ZoneInformation zone in _zones)
        {
            if (zone == null)
                continue;

            //zone.ResetRuntimeData();
        }

        Debug.Log("Runtime-данные всех зон сброшены.");
    }
}