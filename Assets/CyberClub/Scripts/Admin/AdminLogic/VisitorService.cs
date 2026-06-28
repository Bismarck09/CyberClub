using System;
using System.Collections;
using UnityEngine;

public class VisitorService : MonoBehaviour
{
    [SerializeField] private DeviceRegistry _deviceRegistry;
    [SerializeField] private VisitorQueue _visitorQueue;
    [SerializeField] private float _sessionTime = 15f;

    [Header("Optional. If empty, service uses SpeedPotionEffectService.Current")]
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

            if (visitor == null || freeDevice == null || freeDevice.Device == null)
                continue;

            if (!freeDevice.Device.TryReserve())
                continue;

            StartCoroutine(Service(admin, visitor, freeDevice));
        }
    }

    private IEnumerator Service(AdminWorker admin, Visitor visitor, DeviceEntry freeDevice)
    {
        admin.SetBusy(true);

        float adminMultiplier = GetAdminServiceMultiplier();
        float serviceDelay = admin.GetServiceInterval() / Mathf.Max(1f, adminMultiplier);

        yield return new WaitForSeconds(serviceDelay);

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

        float sessionMultiplier = GetDeviceSessionMultiplier();
        float actualSessionTime = _sessionTime / Mathf.Max(1f, sessionMultiplier);

        Debug.Log(
            $"VisitorService: serviceDelay={serviceDelay}, " +
            $"sessionTime={actualSessionTime}, " +
            $"adminMultiplier={adminMultiplier}, " +
            $"sessionMultiplier={sessionMultiplier}"
        );

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

    private SpeedPotionEffectService GetSpeedService()
    {
        if (_speedPotionEffectService != null)
            return _speedPotionEffectService;

        return SpeedPotionEffectService.Current;
    }

    private float GetAdminServiceMultiplier()
    {
        SpeedPotionEffectService speedService = GetSpeedService();
        return speedService != null ? speedService.AdminServiceMultiplier : 1f;
    }

    private float GetDeviceSessionMultiplier()
    {
        SpeedPotionEffectService speedService = GetSpeedService();
        return speedService != null ? speedService.DeviceSessionMultiplier : 1f;
    }
}
