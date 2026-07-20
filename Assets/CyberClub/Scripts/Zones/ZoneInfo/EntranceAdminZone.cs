using System.Collections.Generic;
using UnityEngine;

public class EntranceAdminZone : MonoBehaviour
{
    [SerializeField] private AdminUpgradePanelView _panelView;
    [SerializeField] private AdminUpgradeButton _adminUpgradeButtonController;
    [SerializeField] private AdminSelection _adminSelection;

    private readonly HashSet<Collider> _playerColliders = new();

    public bool IsPlayerInside { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
            return;

        bool wasOutside = _playerColliders.Count == 0;
        _playerColliders.Add(other);

        if (wasOutside)
        {
            SetPlayerInside(true);
            OpenAdminShop();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        _playerColliders.Remove(other);

        if (_playerColliders.Count > 0)
            return;

        SetPlayerInside(false);
        CloseAdminShop();
    }

    private void OnDisable()
    {
        _playerColliders.Clear();
        SetPlayerInside(false);
        CloseAdminShop();
    }

    public void SetPlayerInside(bool value)
    {
        IsPlayerInside = value;
    }

    public void OpenAdminShop()
    {
        if (!IsPlayerInside)
            return;

        _adminSelection?.RefreshSelectedAdmin();
        _panelView?.ShowAdminShop();
        _adminUpgradeButtonController?.SetAdminShopOpened(true);
    }

    public void CloseAdminShop()
    {
        // Closing the UI does not change physical presence or clear selection.
        _adminUpgradeButtonController?.SetAdminShopOpened(false);
        _panelView?.HideAdminShop();
    }

    private static bool IsPlayer(Collider other)
    {
        return other != null && other.GetComponentInParent<PlayerMovement>() != null;
    }
}
