using UnityEngine;
using UnityEngine.UI;

public class AdminUpgradePanelView : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject _scrollView;

    [Header("Cards / Buttons")]
    [SerializeField] private GameObject _deviceCard;
    [SerializeField] private GameObject _adminHireCard;
    [SerializeField] private GameObject _interiorCard;
    [SerializeField] private GameObject _adminUpgradeCard;

    [Header("Upgrade Button")]
    [SerializeField] private Button _adminUpgradeButton;

    public void ShowAdminShop()
    {
        if (_scrollView != null)
            _scrollView.SetActive(true);

        SetDeviceCard(false);
        SetInteriorCard(false);
        SetAdminHireCard(true);
        SetAdminUpgradeCard(false);
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

        if (_adminUpgradeButton != null)
            _adminUpgradeButton.interactable = true;
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

        if (_adminUpgradeButton != null)
            _adminUpgradeButton.interactable = value;
    }
}