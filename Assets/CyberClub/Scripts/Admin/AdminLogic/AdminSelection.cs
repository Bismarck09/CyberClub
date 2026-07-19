using System.Collections.Generic;
using UnityEngine;

public class AdminSelection : MonoBehaviour
{
    [SerializeField] private List<AdminWorker> _admins = new();
    [SerializeField] private AdminUpgradePurchase _adminUpgradePurchase;
    [SerializeField, Min(0f)] private float _selectionHysteresis = 0.35f;

    private readonly HashSet<Collider> _playerColliders = new();
    private Transform _player;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
            return;

        _playerColliders.Add(other);
        _player = other.GetComponentInParent<PlayerMovement>().transform;
        SelectClosestAdmin();
    }

    // ИЗМЕНЕНО: восстанавливает состояние, если администратор
    // был нанят, пока игрок уже находился внутри триггера.
    private void OnTriggerStay(Collider other)
    {
        if (!IsPlayer(other))
            return;

        _playerColliders.Add(other);
        _player = other.GetComponentInParent<PlayerMovement>().transform;
        SelectClosestAdmin();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
            return;

        _playerColliders.Remove(other);

        // ИЗМЕНЕНО: один коллайдер вышел, но другие
        // всё ещё могут находиться в зоне.
        if (_playerColliders.Count > 0)
            return;

        _player = null;
        _adminUpgradePurchase?.ClearSelectedAdmin();
    }

    private void Update()
    {
        if (_player != null && _playerColliders.Count > 0)
            SelectClosestAdmin();
    }

    private void OnDisable()
    {
        _playerColliders.Clear();
        _player = null;
        _adminUpgradePurchase?.ClearSelectedAdmin();
    }

    private void SelectClosestAdmin()
    {
        if (_player == null || _adminUpgradePurchase == null)
            return;

        AdminWorker closest = null;
        float closestDistance = float.MaxValue;

        foreach (AdminWorker admin in _admins)
        {
            if (admin == null || !admin.IsHired || !admin.gameObject.activeInHierarchy)
                continue;

            float distance = Vector3.Distance(_player.position, admin.transform.position);

            if (distance >= closestDistance)
                continue;

            closest = admin;
            closestDistance = distance;
        }

        AdminWorker current = _adminUpgradePurchase.SelectedAdmin;

        if (closest == null)
        {
            _adminUpgradePurchase.ClearSelectedAdmin();
            return;
        }

        if (current != null && current.IsHired && current.gameObject.activeInHierarchy)
        {
            float currentDistance = Vector3.Distance(_player.position, current.transform.position);

            // ИЗМЕНЕНО: гистерезис не даёт карточке дрожать на границе
            // между двумя близко стоящими администраторами.
            if (closest != current && closestDistance + _selectionHysteresis >= currentDistance)
                return;
        }

        _adminUpgradePurchase.SelectAdmin(closest);
    }

    private bool IsPlayer(Collider other)
    {
        // ИЗМЕНЕНО: PlayerMovement может находиться
        // на родительском объекте, а Collider — на дочернем.
        return other != null &&
               other.GetComponentInParent<PlayerMovement>() != null;
    }
}
