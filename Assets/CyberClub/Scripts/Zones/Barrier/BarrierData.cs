using TMPro;
using UnityEngine;

public class BarrierData : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _price;
    [SerializeField] private TextMeshProUGUI _name;

    [SerializeField] private ZoneInformation _zoneInformation;

    private ZonePurchaseConfig _zonePurchaseConfig;

    public ZoneInformation ZoneInformation => _zoneInformation;

    void Start()
    {
        _zonePurchaseConfig = GetComponent<ZonePurchaseConfig>();
        SetTextData();
    }

    private void SetTextData()
    {
        _price.text = ResourceValueFormatter.Format(_zonePurchaseConfig.ZonePrice);
        _name.text = _zoneInformation.ZoneName;
        _name.color = _zoneInformation.ZoneColor;
    }
}
