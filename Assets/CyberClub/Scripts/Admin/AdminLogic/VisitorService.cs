using System;
using System.Collections;
using UnityEngine;

public class VisitorService : MonoBehaviour
{
    [SerializeField] private DeviceRegistry _deviceRegistry;
    [SerializeField] private VisitorQueue _visitorQueue;
    [SerializeField] private float _sessionTime = 15f;
    [SerializeField] private SpeedPotionEffectService _speedPotionEffectService;

    public event Action<DeviceEntry> OnVisitorServiced;

    private void Update()
    {
        ServiceVisitors();
    }

    private void ServiceVisitors()
    {
        foreach (AdminWorker admin in _visitorQueue.GetAdmins())
        {
            if (admin == null || admin.IsHired == false || admin.IsBusy)
                continue;

            Visitor visitor = _visitorQueue.GetNextVisitor(admin);
            DeviceEntry freeDevice = _deviceRegistry.GetRandomFreeDevice();

            if (visitor == null || freeDevice == null)
                continue;

            if (!freeDevice.Device.TryReserve())
                continue;

            StartCoroutine(Service(admin, visitor, freeDevice));
        }
    }

    private IEnumerator Service(AdminWorker admin, Visitor visitor, DeviceEntry freeDevice)
    {
        admin.SetBusy(true);

        float adminMultiplier = _speedPotionEffectService != null
            ? _speedPotionEffectService.AdminServiceMultiplier
            : 1f;

        yield return new WaitForSeconds(admin.GetServiceInterval() / Mathf.Max(1f, adminMultiplier));

        if (visitor == null || freeDevice == null || freeDevice.Device == null)
        {
            if (freeDevice != null && freeDevice.Device != null)
                freeDevice.Device.Release();

            admin.SetBusy(false);
            yield break;
        }

        _visitorQueue.RemoveVisitor(admin, visitor);

        VisitorMovement movement = visitor.GetComponent<VisitorMovement>();
        VisitorExit visitorExit = visitor.GetComponent<VisitorExit>();
        VisitorSeat seatController = visitor.GetComponent<VisitorSeat>();
        GameDevice device = freeDevice.Device;

        if (movement == null || visitorExit == null || device.TargetPoint == null)
        {
            device.Release();
            admin.SetBusy(false);
            yield break;
        }

        float sessionMultiplier = _speedPotionEffectService != null
            ? _speedPotionEffectService.DeviceSessionMultiplier
            : 1f;

        float actualSessionTime = _sessionTime / Mathf.Max(1f, sessionMultiplier);

        movement.Move(device.TargetPoint.position, () =>
        {
            if (seatController != null && device.SitPoint != null)
            {
                seatController.SitAt(device);
                device.StartSession(actualSessionTime, visitorExit, seatController);
            }
            else
            {
                device.Reserve(actualSessionTime, visitorExit);
            }
        });

        OnVisitorServiced?.Invoke(freeDevice);
        admin.SetBusy(false);
    }
}
