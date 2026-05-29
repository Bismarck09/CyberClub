using System;
using UnityEngine;

[Serializable]
public class AdminLevelData
{
    [SerializeField] private int _upgradePrice;
    [SerializeField] private float _serviceInterval;

    public int UpgradePrice => _upgradePrice;
    public float ServiceInterval => _serviceInterval;
}
