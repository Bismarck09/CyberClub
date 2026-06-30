using UnityEngine;

public class EntranceAdminZone : MonoBehaviour
{
    [SerializeField] private AdminUpgradePanelView _panelView;
    [SerializeField] private AdminUpgradePurchase _adminUpgradePurchase;
    [SerializeField] private AdminUpgradeButton _adminUpgradeButtonController;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
            return;

        _panelView.ShowAdminShop();

        if (_adminUpgradeButtonController != null)
            _adminUpgradeButtonController.SetAdminShopOpened(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        if (_adminUpgradePurchase != null)
            _adminUpgradePurchase.ClearSelectedAdmin();

        if (_adminUpgradeButtonController != null)
            _adminUpgradeButtonController.SetAdminShopOpened(false);

        _panelView.HideAdminShop();
    }

    private bool IsPlayer(Collider other)
    {
        return other.TryGetComponent(out PlayerMovement player);
    }
}