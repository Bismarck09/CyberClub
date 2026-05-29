using System;
using UnityEngine;

public class AdminSelection : MonoBehaviour
{
    [SerializeField] private AdminWorker _admin;

    public static event Action<AdminWorker> OnAdminSelected;
    public static event Action<AdminWorker> OnAdminDeselected;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out PlayerMovement player))
            return;

        if (_admin == null || _admin.IsHired == false)
            return;

        OnAdminSelected?.Invoke(_admin);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out PlayerMovement player))
            return;

        if (_admin == null)
            return;

        OnAdminDeselected?.Invoke(_admin);
    }
}
