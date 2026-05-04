using TMPro;
using UnityEngine;

public class ZoneInfoPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _zoneNameText;
    [SerializeField] private ZoneSwitcher _zoneSwitcher;

    private void OnEnable()
    {
        _zoneSwitcher.OnZoneChanged += UpdateZoneInfo;
        _zoneSwitcher.OnZoneExited += ClearZoneInfo;
    }

    private void OnDisable()
    {
        _zoneSwitcher.OnZoneChanged -= UpdateZoneInfo;
        _zoneSwitcher.OnZoneExited -= ClearZoneInfo;
    }

    private void UpdateZoneInfo(ZoneInformation zoneInformation)
    {
        _zoneNameText.text = zoneInformation.ZoneName;
        _zoneNameText.color = zoneInformation.ZoneColor;
    }

    private void ClearZoneInfo()
    {
        _zoneNameText.text = "Холл";
        _zoneNameText.color = Color.white;
    }

}
