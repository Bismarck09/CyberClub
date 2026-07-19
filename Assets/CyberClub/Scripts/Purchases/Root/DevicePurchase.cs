using System;
using UnityEngine;

public class DevicePurchase : MonoBehaviour, IPurchasable
{
    private const float PurchaseDebounceSeconds = 0.25f;

    [SerializeField] private CoinsData _coinsData;
    [SerializeField] private ZoneSwitcher _zoneSwitcher;
    [SerializeField] private SaveLoadManager _saveLoadManager;
    [SerializeField] private PurchaseFeedbackPresenter _feedbackPresenter;

    private ZoneInformation _zoneInformation;
    private DeviceSpawner _deviceSpawner;
    private bool _isPurchasing;
    private float _nextPurchaseAllowedTime = float.NegativeInfinity;

    public event Action OnDevicePurchased;
    public event Action OnDeviceStateChanged;
    public event Action<int> OnDevicePriceChanged;

    public int CurrentDevicePrice => _zoneInformation != null ? _zoneInformation.CurrentDevicePrice : 0;

    public bool IsDeviceLimitReached
    {
        get
        {
            ZoneInformation zoneInformation = _zoneInformation;

            if (zoneInformation == null || _deviceSpawner == null)
                return true;

            return !_deviceSpawner.CanSpawnDevice(zoneInformation);
        }
    }

    private void OnEnable()
    {
        if (_zoneSwitcher != null)
            _zoneSwitcher.OnZoneChanged += UpdateZoneInformation;
    }

    private void OnDisable()
    {
        if (_zoneSwitcher != null)
            _zoneSwitcher.OnZoneChanged -= UpdateZoneInformation;
    }

    public bool CanBuy()
    {
        ZoneInformation zoneInformation = _zoneInformation;

        if (_isPurchasing || !CanCreateDevice(zoneInformation) || _coinsData == null)
            return false;

        int price = zoneInformation.CurrentDevicePrice;
        return price >= 0 && _coinsData.CurrentCoins >= price;
    }

    public void Buy()
    {
        if (_isPurchasing || Time.unscaledTime < _nextPurchaseAllowedTime)
            return;

        ZoneInformation zoneInformation = _zoneInformation;
        int price = zoneInformation != null ? zoneInformation.CurrentDevicePrice : 0;

        if (zoneInformation == null || _deviceSpawner == null)
        {
            Fail(PurchaseFailureReason.ProductUnavailable);
            return;
        }

        if (!_deviceSpawner.CanSpawnDevice(zoneInformation))
        {
            Fail(PurchaseFailureReason.MaximumReached);
            return;
        }

        if (_coinsData == null || price < 0)
        {
            Fail(PurchaseFailureReason.TransactionFailed);
            return;
        }

        if (_coinsData.CurrentCoins < price)
        {
            Fail(PurchaseFailureReason.NotEnoughCoins);
            return;
        }

        int coinsBeforePurchase = _coinsData.CurrentCoins;
        _isPurchasing = true;
        bool deviceCreated = false;

        try
        {
            if (!_coinsData.TryBuy(price))
            {
                Fail(PurchaseFailureReason.NotEnoughCoins);
                return;
            }

            deviceCreated = _deviceSpawner.TrySpawnDevice(zoneInformation, out _);

            if (!deviceCreated)
            {
                Fail(PurchaseFailureReason.TransactionFailed);
                return;
            }

            zoneInformation.RegisterDevicePurchase();
            // ИЗМЕНЕНО: повторный UI-click после синхронной транзакции не создаёт второе устройство.
            _nextPurchaseAllowedTime = Time.unscaledTime + PurchaseDebounceSeconds;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
            Fail(PurchaseFailureReason.TransactionFailed);
            return;
        }
        finally
        {
            if (!deviceCreated)
            {
                int missingCoins = Mathf.Max(0, coinsBeforePurchase - _coinsData.CurrentCoins);
                RefundCoins(missingCoins);
            }

            _isPurchasing = false;
        }

        InvokeSafely(OnDevicePurchased);
        NotifyDeviceStateChanged();
        // ИЗМЕНЕНО: покупка устройства сохраняется сразу, включая закрытие игры после первого ПК.
        _saveLoadManager?.SaveGame();
    }

    public void RegisterDeviceSpawner(DeviceSpawner deviceSpawner)
    {
        if (deviceSpawner == null)
            return;

        if (_deviceSpawner != null && _deviceSpawner != deviceSpawner)
        {
            Debug.LogError("DevicePurchase: зарегистрировано несколько DeviceSpawner.", this);
            return;
        }

        _deviceSpawner = deviceSpawner;
        NotifyDeviceStateChanged();
    }

    public void UnregisterDeviceSpawner(DeviceSpawner deviceSpawner)
    {
        if (_deviceSpawner != deviceSpawner)
            return;

        _deviceSpawner = null;
        NotifyDeviceStateChanged();
    }

    private void UpdateZoneInformation(ZoneInformation zoneInformation)
    {
        _zoneInformation = zoneInformation;
        NotifyDeviceStateChanged();
    }

    private void NotifyDeviceStateChanged()
    {
        InvokeSafely(OnDevicePriceChanged, CurrentDevicePrice);
        InvokeSafely(OnDeviceStateChanged);
    }

    private bool CanCreateDevice(ZoneInformation zoneInformation)
    {
        if (zoneInformation == null || _deviceSpawner == null)
            return false;

        return _deviceSpawner.CanSpawnDevice(zoneInformation);
    }

    private void RefundCoins(int amount)
    {
        if (amount <= 0 || _coinsData == null)
            return;

        try
        {
            _coinsData.AddResource(amount, 1f);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
        }
    }

    private void InvokeSafely(Action callback)
    {
        if (callback == null)
            return;

        foreach (Delegate handler in callback.GetInvocationList())
        {
            try
            {
                ((Action)handler).Invoke();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }
    }

    private void InvokeSafely(Action<int> callback, int value)
    {
        if (callback == null)
            return;

        foreach (Delegate handler in callback.GetInvocationList())
        {
            try
            {
                ((Action<int>)handler).Invoke(value);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }
    }

    private void Fail(PurchaseFailureReason reason)
    {
        _feedbackPresenter?.Show(reason);
    }
}
