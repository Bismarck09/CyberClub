using System;
using UnityEngine;

public class AdminUpgradePurchase : MonoBehaviour, IPurchasable
{
    [SerializeField] private CoinsData _coinsData;

    private AdminWorker _selectedAdmin;

    public AdminWorker SelectedAdmin => _selectedAdmin;

    public event Action<AdminWorker> OnSelectedAdminChanged;
    public event Action<AdminWorker> OnAdminUpgraded;

    private void OnEnable()
    {
        AdminSelection.OnAdminSelected += SelectAdmin;
        AdminSelection.OnAdminDeselected += DeselectAdmin;
    }

    private void OnDisable()
    {
        AdminSelection.OnAdminSelected -= SelectAdmin;
        AdminSelection.OnAdminDeselected -= DeselectAdmin;
    }

    public bool CanBuy()
    {
        if (_selectedAdmin == null)
            return false;

        if (_selectedAdmin.IsHired == false)
            return false;

        if (_selectedAdmin.CanUpgrade() == false)
            return false;

        return _coinsData.TryBuy(_selectedAdmin.GetUpgradePrice());
    }

    public void Buy()
    {
        if (!CanBuy())
            return;

        _selectedAdmin.Upgrade();
        OnAdminUpgraded?.Invoke(_selectedAdmin);
    }

    public void ClearSelectedAdmin()
    {
        _selectedAdmin = null;
        OnSelectedAdminChanged?.Invoke(null);
    }

    private void SelectAdmin(AdminWorker admin)
    {
        _selectedAdmin = admin;
        OnSelectedAdminChanged?.Invoke(_selectedAdmin);
    }

    private void DeselectAdmin(AdminWorker admin)
    {
        if (_selectedAdmin != admin)
            return;

        ClearSelectedAdmin();
    }
}