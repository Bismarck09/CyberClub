using System;
using System.Collections.Generic;
using UnityEngine;

public class AdminSelection : MonoBehaviour
{
    [SerializeField] private AdminWorker _admin;

    public static event Action<AdminWorker> OnAdminSelected;
    public static event Action<AdminWorker> OnAdminDeselected;

    // ИЗМЕНЕНО: учитываем все коллайдеры игрока.
    private readonly HashSet<Collider> _playerColliders = new();

    private bool _isSelected;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
            return;

        _playerColliders.Add(other);
        TrySelectAdmin();
    }

    // ИЗМЕНЕНО: восстанавливает состояние, если администратор
    // был нанят, пока игрок уже находился внутри триггера.
    private void OnTriggerStay(Collider other)
    {
        if (!IsPlayer(other))
            return;

        _playerColliders.Add(other);
        TrySelectAdmin();
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

        DeselectAdmin();
    }

    private void OnDisable()
    {
        _playerColliders.Clear();
        DeselectAdmin();
    }

    private void TrySelectAdmin()
    {
        if (_isSelected)
            return;

        if (_admin == null || !_admin.IsHired)
            return;

        _isSelected = true;
        OnAdminSelected?.Invoke(_admin);
    }

    private void DeselectAdmin()
    {
        if (!_isSelected)
            return;

        _isSelected = false;

        if (_admin != null)
            OnAdminDeselected?.Invoke(_admin);
    }

    private bool IsPlayer(Collider other)
    {
        // ИЗМЕНЕНО: PlayerMovement может находиться
        // на родительском объекте, а Collider — на дочернем.
        return other != null &&
               other.GetComponentInParent<PlayerMovement>() != null;
    }
}