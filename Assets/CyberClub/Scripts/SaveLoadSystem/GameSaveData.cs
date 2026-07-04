using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveData
{
    public int Version = 1;
    public ResourceSaveData Resources = new();
    public RatingSaveData Rating = new();
    public List<ZoneSaveData> Zones = new();
    public List<AdminSaveData> Admins = new();
    public SettingsSaveData Settings = new();
}

[Serializable] public class ResourceSaveData { public int Coins; public int Gems; }
[Serializable] public class RatingSaveData { public float CurrentRating; }

[Serializable]
public class ZoneSaveData
{
    public string Id;
    public bool IsUnlocked;
    public int PurchasedDevices;
    public int PurchasedInteriorObjects;
}

[Serializable]
public class AdminSaveData
{
    public string Id;
    public bool IsHired;
    public int LevelIndex;
}

[Serializable]
public class SettingsSaveData
{
    public bool HasSettings;
    public bool MusicEnabled = true;
    public bool EffectsEnabled = true;
    public float MusicVolume = 0.5f;
    public float EffectsVolume = 0.5f;
}