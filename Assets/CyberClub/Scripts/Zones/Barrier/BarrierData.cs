using TMPro;
using UnityEngine;

public class BarrierData : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _price;
    [SerializeField] private TextMeshProUGUI _name;

    [SerializeField] private ZoneInformation _zoneInformation;

    private ZonePurchaseConfig _zonePurchaseConfig;

    void Start()
    {
        _zonePurchaseConfig = GetComponent<ZonePurchaseConfig>();
        SetTextData();
    }

    private void SetTextData()
    {
        _price.text = _zonePurchaseConfig.ZonePrice.ToString();
        _name.text = _zoneInformation.ZoneName;
        _name.color = _zoneInformation.ZoneColor;
    }
}
