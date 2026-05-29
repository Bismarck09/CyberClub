using UnityEngine;

public class AdminUpgradeButton : MonoBehaviour
{
    [SerializeField] private AdminUpgradePanelView _panelView;
    [SerializeField] private AdminUpgradePurchase _adminUpgradePurchase;

    private bool _isAdminShopOpened;

    private void OnEnable()
    {
        AdminSelection.OnAdminSelected += OnAdminSelected;
        AdminSelection.OnAdminDeselected += OnAdminDeselected;
    }

    private void OnDisable()
    {
        AdminSelection.OnAdminSelected -= OnAdminSelected;
        AdminSelection.OnAdminDeselected -= OnAdminDeselected;
    }

    public void SetAdminShopOpened(bool value)
    {
        _isAdminShopOpened = value;

        if (_isAdminShopOpened == false)
            _panelView.HideAdminUpgrade();
    }

    private void OnAdminSelected(AdminWorker admin)
    {
        if (_isAdminShopOpened == false)
            return;

        if (admin == null || admin.IsHired == false)
            return;

        _panelView.ShowAdminUpgrade();
    }

    private void OnAdminDeselected(AdminWorker admin)
    {
        if (_isAdminShopOpened == false)
            return;

        _panelView.HideAdminUpgrade();
    }
}