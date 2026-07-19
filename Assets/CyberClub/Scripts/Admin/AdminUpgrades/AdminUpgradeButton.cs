using UnityEngine;

public class AdminUpgradeButton : MonoBehaviour
{
    [SerializeField] private AdminUpgradePanelView _panelView;
    [SerializeField] private AdminUpgradePurchase _adminUpgradePurchase;

    private bool _isAdminShopOpened;
    private AdminWorker _currentAdmin;

    private void OnEnable()
    {
        if (_adminUpgradePurchase != null)
            _adminUpgradePurchase.OnSelectedAdminChanged += OnAdminSelected;
    }

    private void OnDisable()
    {
        if (_adminUpgradePurchase != null)
            _adminUpgradePurchase.OnSelectedAdminChanged -= OnAdminSelected;
    }

    public void SetAdminShopOpened(bool value)
    {
        _isAdminShopOpened = value;

        // ИЗМЕНЕНО: при повторном открытии получаем уже
        // выбранного администратора, не ожидая нового OnTriggerEnter.
        if (_isAdminShopOpened &&
            _adminUpgradePurchase != null)
        {
            _currentAdmin =
                _adminUpgradePurchase.SelectedAdmin;
        }

        RefreshPanel();
    }

    private void OnAdminSelected(AdminWorker admin)
    {
        _currentAdmin = admin;
        RefreshPanel();
    }

    private void RefreshPanel()
    {
        if (_panelView == null)
            return;

        bool shouldShow =
            _isAdminShopOpened &&
            _currentAdmin != null &&
            _currentAdmin.IsHired;

        if (shouldShow)
            _panelView.ShowAdminUpgrade();
        else
            _panelView.HideAdminUpgrade();
    }
}
