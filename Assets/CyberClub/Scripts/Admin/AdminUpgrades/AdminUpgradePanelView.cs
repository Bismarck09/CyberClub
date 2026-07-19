using UnityEngine;

public class AdminUpgradePanelView : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject _scrollView;

    [Header("Cards / Buttons")]
    [SerializeField] private GameObject _deviceCard;
    [SerializeField] private GameObject _adminHireCard;
    [SerializeField] private GameObject _interiorCard;
    [SerializeField] private GameObject _adminUpgradeCard;

    public void ShowAdminShop()
    {
        if (_scrollView != null)
            _scrollView.SetActive(true);

        SetDeviceCard(false);
        SetInteriorCard(false);
        SetAdminHireCard(true);
        // AdminUpgradeButton is the single owner of the selected-admin card.
        // Hiding it here made callback order decide whether the card stayed visible.
    }

    public void HideAdminShop()
    {
        SetDeviceCard(true);
        SetInteriorCard(true);
        SetAdminHireCard(false);
        SetAdminUpgradeCard(false);

        if (_scrollView != null)
            _scrollView.SetActive(false);
    }

    public void ShowAdminUpgrade()
    {
        SetAdminUpgradeCard(true);
    }

    public void HideAdminUpgrade()
    {
        SetAdminUpgradeCard(false);
    }

    private void SetDeviceCard(bool value)
    {
        if (_deviceCard != null)
            _deviceCard.SetActive(value);
    }

    private void SetInteriorCard(bool value)
    {
        if (_interiorCard != null)
            _interiorCard.SetActive(value);
    }

    private void SetAdminHireCard(bool value)
    {
        if (_adminHireCard != null)
            _adminHireCard.SetActive(value);
    }

    private void SetAdminUpgradeCard(bool value)
    {
        if (_adminUpgradeCard != null)
            _adminUpgradeCard.SetActive(value);
    }
}
