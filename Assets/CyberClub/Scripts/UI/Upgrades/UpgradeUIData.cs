using TMPro;
using UnityEngine;

public class UpgradeUIData : MonoBehaviour
{
    private const string MaxText = "MAX";

    [SerializeField] private TextMeshProUGUI _devicePriceText;
    [SerializeField] private ZoneSwitcher _zoneSwitcher;
    [SerializeField] private InteriorPurchase _interiorPurchase;
    [SerializeField] private DevicePurchase _devicePurchase;
    [SerializeField] private TextMeshProUGUI _interiorPrice;
    [SerializeField] private TextMeshProUGUI _interiorMultiplier;

    private ZoneInformation _zoneInformation;
    private int _lastDevicePrice = -1;
    private bool _lastDeviceLimitState;

    private void OnEnable()
    {
        if (_zoneSwitcher != null)
            _zoneSwitcher.OnZoneChanged += UpdateUpgradeUI;

        if (_interiorPurchase != null)
            _interiorPurchase.OnInteriorPurchase += ChangeInteriorData;

        if (_devicePurchase != null)
        {
            _devicePurchase.OnDevicePurchased += ForceRefreshDeviceData;
            _devicePurchase.OnDeviceStateChanged += ForceRefreshDeviceData;
            _devicePurchase.OnDevicePriceChanged += ForceRefreshDeviceData;
        }

        ForceRefreshDeviceData();
    }

    private void OnDisable()
    {
        if (_zoneSwitcher != null)
            _zoneSwitcher.OnZoneChanged -= UpdateUpgradeUI;

        if (_interiorPurchase != null)
            _interiorPurchase.OnInteriorPurchase -= ChangeInteriorData;

        if (_devicePurchase != null)
        {
            _devicePurchase.OnDevicePurchased -= ForceRefreshDeviceData;
            _devicePurchase.OnDeviceStateChanged -= ForceRefreshDeviceData;
            _devicePurchase.OnDevicePriceChanged -= ForceRefreshDeviceData;
        }
    }

    private void Update()
    {
        // Страховка: даже если какое-то событие не пришло из-за порядка OnEnable,
        // UI всё равно обновится, когда цена или состояние лимита изменятся.
        if (_devicePurchase == null)
            return;

        int price = _devicePurchase.CurrentDevicePrice;
        bool isLimitReached = _devicePurchase.IsDeviceLimitReached;

        if (price == _lastDevicePrice && isLimitReached == _lastDeviceLimitState)
            return;

        ChangeDeviceData();
    }

    private void UpdateUpgradeUI(ZoneInformation zoneInformation)
    {
        _zoneInformation = zoneInformation;

        ChangeDeviceData();

        if (_zoneInformation != null && _zoneInformation.Interior != null)
            ChangeInteriorData(_zoneInformation.Interior);
    }

    private void ChangeInteriorData(InteriorData interiorData)
    {
        if (interiorData == null)
            return;

        if (_interiorPrice != null)
        {
            _interiorPrice.text = interiorData.IsMaxPurchased
                ? MaxText
                : ResourceValueFormatter.Format(interiorData.InteriorsPrice);
        }

        if (_interiorMultiplier != null)
            _interiorMultiplier.text = interiorData.GetCoinsMultiplier().ToString("0.##");
    }

    private void ForceRefreshDeviceData()
    {
        ChangeDeviceData();
    }

    private void ForceRefreshDeviceData(int unusedPrice)
    {
        ChangeDeviceData();
    }

    private void ChangeDeviceData()
    {
        if (_devicePriceText == null)
            return;

        if (_zoneInformation == null || _devicePurchase == null)
        {
            _devicePriceText.text = "0";
            _lastDevicePrice = -1;
            _lastDeviceLimitState = false;
            return;
        }

        _lastDevicePrice = _devicePurchase.CurrentDevicePrice;
        _lastDeviceLimitState = _devicePurchase.IsDeviceLimitReached;

        _devicePriceText.text = _lastDeviceLimitState
            ? MaxText
            : ResourceValueFormatter.Format(_devicePurchase.CurrentDevicePrice);
    }
}
