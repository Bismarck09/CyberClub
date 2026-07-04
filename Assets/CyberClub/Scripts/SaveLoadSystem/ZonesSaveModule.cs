using System;
using System.Collections.Generic;
using UnityEngine;

public class ZonesSaveModule : MonoBehaviour, ISaveModule
{
    [SerializeField] private List<ZoneSaveSlot> _zones = new();

    public void Capture(GameSaveData saveData)
    {
        saveData.Zones.Clear();

        foreach (ZoneSaveSlot slot in _zones)
        {
            if (slot == null || slot.Zone == null || string.IsNullOrWhiteSpace(slot.Id))
                continue;

            saveData.Zones.Add(new ZoneSaveData
            {
                Id = slot.Id,
                IsUnlocked = slot.IsUnlocked,
                PurchasedDevices = slot.Zone.CurrentDevicePurchases,
                PurchasedInteriorObjects = slot.Zone.Interior != null ? slot.Zone.Interior.CurrentBoughtInteriorObjects : 0
            });
        }
    }

    public void Restore(GameSaveData saveData)
    {
        foreach (ZoneSaveSlot slot in _zones)
        {
            if (slot == null || slot.Zone == null || string.IsNullOrWhiteSpace(slot.Id))
                continue;

            ZoneSaveData data = saveData.Zones.Find(zone => zone.Id == slot.Id);

            if (data == null)
            {
                slot.ApplyUnlockedState(slot.IsUnlockedByDefault);
                slot.Zone.RestoreDevicePurchases(0);

                if (slot.Zone.Interior != null)
                    slot.Zone.Interior.RestoreBoughtInteriorObjects(0);

                continue;
            }

            slot.ApplyUnlockedState(data.IsUnlocked);
            slot.Zone.RestoreDevicePurchases(data.PurchasedDevices);

            if (slot.Zone.Interior != null)
                slot.Zone.Interior.RestoreBoughtInteriorObjects(data.PurchasedInteriorObjects);

            if (slot.DeviceSpawner != null && data.PurchasedDevices > 0)
                slot.DeviceSpawner.RestoreDevices(slot.Zone, data.PurchasedDevices);
        }
    }
}