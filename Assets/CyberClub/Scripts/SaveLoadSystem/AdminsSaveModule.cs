using System;
using System.Collections.Generic;
using UnityEngine;

public class AdminsSaveModule : MonoBehaviour, ISaveModule
{
    [SerializeField] private List<AdminSaveSlot> _admins = new();

    public void Capture(GameSaveData saveData)
    {
        saveData.Admins.Clear();

        foreach (AdminSaveSlot slot in _admins)
        {
            if (slot == null || slot.Admin == null || string.IsNullOrWhiteSpace(slot.Id))
                continue;

            saveData.Admins.Add(new AdminSaveData
            {
                Id = slot.Id,
                IsHired = slot.Admin.IsHired,
                LevelIndex = slot.Admin.LevelIndex
            });
        }
    }

    public void Restore(GameSaveData saveData)
    {
        foreach (AdminSaveSlot slot in _admins)
        {
            if (slot == null || slot.Admin == null || string.IsNullOrWhiteSpace(slot.Id))
                continue;

            AdminSaveData data = saveData.Admins.Find(admin => admin.Id == slot.Id);

            if (data == null)
            {
                slot.Admin.RestoreState(slot.IsHiredByDefault, 0);
                continue;
            }

            slot.Admin.RestoreState(data.IsHired, data.LevelIndex);
        }
    }
}

[Serializable]
public class AdminSaveSlot
{
    [SerializeField] private string _id;
    [SerializeField] private AdminWorker _admin;
    [SerializeField] private bool _isHiredByDefault;

    public string Id => _id;
    public AdminWorker Admin => _admin;
    public bool IsHiredByDefault => _isHiredByDefault;
}