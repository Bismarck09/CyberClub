using System;
using System.Collections;
using UnityEngine;

public class VisitorService : MonoBehaviour
{
    [SerializeField] private DeviceRegistry _deviceRegistry;
    [SerializeField] private VisitorQueue _visitorQueue;
    [SerializeField] private float _sessionTime = 8f;

    [Header("Speed potion")]
    [SerializeField] private SpeedPotionEffectService _speedPotionEffectService;

    [Header("Rating")]
    [SerializeField] private RatingData _ratingData;
    [SerializeField] private float _goodQueueWaitTime = 3f;
    [SerializeField] private float _positiveRatingChange = 0.05f;
    [SerializeField] private float _negativeRatingChange = 0.08f;
    [SerializeField] private float _extraPenaltyEverySeconds = 3f;
    [SerializeField] private float _extraPenaltyAmount = 0.03f;

    public event Action<DeviceEntry> OnVisitorServiced;

    private void Update()
    {
        ServiceVisitors();
    }

    private void ServiceVisitors()
    {
        if (_visitorQueue == null || _deviceRegistry == null)
            return;

        foreach (AdminWorker admin in _visitorQueue.GetAdmins())
        {
            if (admin == null ||
                !admin.IsHired ||
                admin.IsBusy)
            {
                continue;
            }

            Visitor visitor =
                _visitorQueue.GetNextVisitor(admin);

            DeviceEntry freeDevice =
                _deviceRegistry.GetRandomFreeDevice();

            if (visitor == null ||
                freeDevice == null ||
                freeDevice.Device == null)
            {
                continue;
            }

            if (!freeDevice.Device.TryReserve())
                continue;

            VisitorExit visitorExit = visitor.GetComponent<VisitorExit>();

            if (visitorExit == null)
            {
                freeDevice.Device.Release();
                Debug.LogError($"VisitorService: у {visitor.name} отсутствует VisitorExit на этапе резервирования.", visitor);
                continue;
            }

            // ИЗМЕНЕНО: резерв связывается с посетителем до coroutine-задержки,
            // поэтому OnDestroy всегда сможет освободить устройство.
            visitorExit.TrackReservedDevice(freeDevice.Device);

            StartCoroutine(
                Service(admin, visitor, freeDevice));
        }
    }

    private IEnumerator Service(
        AdminWorker admin,
        Visitor visitor,
        DeviceEntry freeDevice)
    {
        admin.SetBusy(true);

        float serviceDelay =
            admin.GetServiceInterval() /
            Mathf.Max(1f, GetAdminServiceMultiplier());

        yield return new WaitForSeconds(serviceDelay);

        if (visitor == null ||
            freeDevice == null ||
            freeDevice.Device == null)
        {
            if (freeDevice?.Device != null)
                freeDevice.Device.Release();

            admin.SetBusy(false);
            yield break;
        }

        EvaluateVisitorRating(visitor);
        _visitorQueue.RemoveVisitor(admin, visitor);

        VisitorMovement movement =
            visitor.GetComponent<VisitorMovement>();

        VisitorExit visitorExit = visitor.GetComponent<VisitorExit>();

        VisitorSeat seatController =
            visitor.GetComponent<VisitorSeat>();

        GameDevice device = freeDevice.Device;

        if (movement == null ||
            visitorExit == null ||
            device.TargetPoint == null)
        {
            device.Release();
            admin.SetBusy(false);

            visitorExit?.MoveToExit();
            yield break;
        }

        float actualSessionTime =
            _sessionTime /
            Mathf.Max(1f, GetDeviceSessionMultiplier());

        bool movementStarted = movement.Move(
            device.TargetPoint.position,
            () =>
            {
                if (seatController != null &&
                    device.SitPoint != null)
                {
                    seatController.SitAt(device);

                    device.StartSession(
                        actualSessionTime,
                        visitorExit,
                        seatController,
                        () => NotifyVisitorServicedSafely(freeDevice));
                }
                else
                {
                    device.Reserve(
                        actualSessionTime,
                        visitorExit,
                        () => NotifyVisitorServicedSafely(freeDevice));
                }
            },
            () =>
            {
                // ИЗМЕНЕНО: раньше компьютер оставался
                // зарезервированным навсегда.
                device.Release();
                visitorExit.ClearReservedDevice(device);
                Debug.LogWarning($"VisitorService: {visitor.name} не дошёл до устройства {device.name}.", visitor);
                visitorExit.MoveToExit();
            });

        if (!movementStarted)
        {
            device.Release();
            visitorExit.ClearReservedDevice(device);
            Debug.LogWarning($"VisitorService: путь к устройству не запущен для {visitor.name}.", visitor);
            visitorExit.MoveToExit();
        }
        admin.SetBusy(false);
    }

    private void NotifyVisitorServicedSafely(DeviceEntry device)
    {
        if (OnVisitorServiced == null)
            return;

        // ИЗМЕНЕНО: доход/туториал уведомляются независимо, а администратор освобождается
        // даже при ошибке одного внешнего подписчика.
        foreach (Delegate handler in OnVisitorServiced.GetInvocationList())
        {
            try
            {
                ((Action<DeviceEntry>)handler).Invoke(device);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }
    }

    private void EvaluateVisitorRating(Visitor visitor)
    {
        if (_ratingData == null || visitor == null)
            return;

        VisitorRatingTracker ratingTracker =
            visitor.GetComponent<VisitorRatingTracker>();

        if (ratingTracker == null)
            return;

        ratingTracker.EvaluateWaitingTime(
            _ratingData,
            _goodQueueWaitTime,
            _positiveRatingChange,
            _negativeRatingChange,
            _extraPenaltyEverySeconds,
            _extraPenaltyAmount);
    }

    private float GetAdminServiceMultiplier()
    {
        if (_speedPotionEffectService != null)
            return _speedPotionEffectService.AdminServiceMultiplier;

        return SpeedPotionEffectService.Current != null
            ? SpeedPotionEffectService.Current.AdminServiceMultiplier
            : 1f;
    }

    private float GetDeviceSessionMultiplier()
    {
        if (_speedPotionEffectService != null)
            return _speedPotionEffectService.DeviceSessionMultiplier;

        return SpeedPotionEffectService.Current != null
            ? SpeedPotionEffectService.Current.DeviceSessionMultiplier
            : 1f;
    }
}
