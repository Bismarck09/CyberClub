using System.Collections.Generic;
using UnityEngine;

public class EntranceAdminZone : MonoBehaviour
{
    [SerializeField] private AdminUpgradePanelView _panelView;
    [SerializeField] private AdminUpgradeButton _adminUpgradeButtonController;

    // ИЗМЕНЕНО: учитываем все коллайдеры игрока.
    private readonly HashSet<Collider> _playerColliders = new();

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
            return;

        bool wasOutside = _playerColliders.Count == 0;
        _playerColliders.Add(other);

        if (!wasOutside)
            return;

        _panelView?.ShowAdminShop();
        _adminUpgradeButtonController?.SetAdminShopOpened(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        _playerColliders.Remove(other);

        if (_playerColliders.Count > 0)
            return;

        CloseAdminShop();
    }

    private void OnDisable()
    {
        _playerColliders.Clear();
        CloseAdminShop();
    }

    private void CloseAdminShop()
    {
        _adminUpgradeButtonController?.SetAdminShopOpened(false);
        _panelView?.HideAdminShop();
    }

    private bool IsPlayer(Collider other)
    {
        return other != null &&
               other.GetComponentInParent<PlayerMovement>() != null;
    }
}
