using System;
using System.Collections.Generic;
using UnityEngine;

public class AdminPurchase : MonoBehaviour, IPurchasable
{
    [SerializeField] private CoinsData _coinsData;
    [SerializeField] private List<AdminWorker> _admins;

    public event Action OnAdminPurchased;

    public bool CanBuy()
    {
        AdminWorker admin = GetNextNotHiredAdmin();

        if (admin == null)
            return false;

        return _coinsData.TryBuy(admin.HirePrice);
    }

    public void Buy()
    {
        AdminWorker admin = GetNextNotHiredAdmin();

        if (admin == null)
            return;

        if (!CanBuy())
            return;

        admin.Hire();
        OnAdminPurchased?.Invoke();
    }

    public AdminWorker GetNextNotHiredAdmin()
    {
        foreach (AdminWorker admin in _admins)
        {
            if (admin != null && admin.IsHired == false)
                return admin;
        }

        return null;
    }
}