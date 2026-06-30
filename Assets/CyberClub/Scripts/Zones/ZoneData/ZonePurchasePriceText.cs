using UnityEngine;
using TMPro;
using UnityEngine;

public class ZonePurchasePriceText : MonoBehaviour
{
    [SerializeField] private ZonePurchaseConfig _zonePurchaseConfig;
    [SerializeField] private TMP_Text _priceText;

    private void Reset()
    {
        _priceText = GetComponent<TMP_Text>();
        _zonePurchaseConfig = GetComponentInParent<ZonePurchaseConfig>();
    }

    private void OnEnable()
    {
        Refresh();
    }

    [ContextMenu("Refresh")]
    public void Refresh()
    {
        if (_priceText == null)
            return;

        if (_zonePurchaseConfig == null)
        {
            _priceText.text = "0";
            return;
        }

        _priceText.text = ResourceValueFormatter.Format(_zonePurchaseConfig.ZonePrice);
    }
}
