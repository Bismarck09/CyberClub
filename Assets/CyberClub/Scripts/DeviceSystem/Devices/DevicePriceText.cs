using TMPro;
using UnityEngine;

public class DevicePriceText : MonoBehaviour
{
    [SerializeField] private DevicePurchase _devicePurchase;
    [SerializeField] private TMP_Text _priceText;
    [SerializeField] private string _unavailableText = "MAX";

    private void OnEnable()
    {
        if (_devicePurchase != null)
            _devicePurchase.OnDevicePriceChanged += UpdateText;

        UpdateText(_devicePurchase != null ? _devicePurchase.CurrentDevicePrice : 0);
    }

    private void OnDisable()
    {
        if (_devicePurchase != null)
            _devicePurchase.OnDevicePriceChanged -= UpdateText;
    }

    private void UpdateText(int price)
    {
        if (_priceText == null)
            return;

        if (_devicePurchase == null || _devicePurchase.IsDeviceLimitReached)
        {
            _priceText.text = _unavailableText;
            return;
        }

        _priceText.text = ResourceValueFormatter.Format(price);
    }
}
