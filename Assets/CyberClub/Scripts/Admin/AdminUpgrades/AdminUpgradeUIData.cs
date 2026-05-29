using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdminUpgradeUIData : MonoBehaviour
{
    [Header("Admin purchase card")]
    [SerializeField] private TextMeshProUGUI _hireTitleText;
    [SerializeField] private TextMeshProUGUI _hirePriceText;
    [SerializeField] private Button _hireButton;

    [Header("Admin upgrade card")]
    [SerializeField] private TextMeshProUGUI _upgradeTitleText;
    [SerializeField] private TextMeshProUGUI _upgradePriceText;
    [SerializeField] private TextMeshProUGUI _upgradeSpeedText;
    [SerializeField] private Button _upgradeButton;

    [Header("Logic")]
    [SerializeField] private AdminPurchase _adminPurchase;
    [SerializeField] private AdminUpgradePurchase _adminUpgradePurchase;
    [SerializeField] private ZoneSwitcher _zoneSwitcher;

    private void OnEnable()
    {
        _zoneSwitcher.OnZoneChanged += OnZoneChanged;
        _zoneSwitcher.OnZoneExited += OnZoneExited;

        _adminPurchase.OnAdminPurchased += UpdateHireCard;
        _adminUpgradePurchase.OnSelectedAdminChanged += UpdateUpgradeCard;
        _adminUpgradePurchase.OnAdminUpgraded += UpdateUpgradeCard;

        UpdateHireCard();
        UpdateUpgradeCard(null);
    }

    private void OnDisable()
    {
        _zoneSwitcher.OnZoneChanged -= OnZoneChanged;
        _zoneSwitcher.OnZoneExited -= OnZoneExited;

        _adminPurchase.OnAdminPurchased -= UpdateHireCard;
        _adminUpgradePurchase.OnSelectedAdminChanged -= UpdateUpgradeCard;
        _adminUpgradePurchase.OnAdminUpgraded -= UpdateUpgradeCard;
    }

    private void OnZoneChanged(ZoneInformation zoneInformation)
    {
        UpdateHireCard();
        UpdateUpgradeCard(_adminUpgradePurchase.SelectedAdmin);
    }

    private void OnZoneExited()
    {
        _upgradeButton.interactable = false;
    }

    private void UpdateHireCard()
    {
        AdminWorker nextAdmin = _adminPurchase.GetNextNotHiredAdmin();

        if (nextAdmin == null)
        {
            _hireTitleText.text = "Все админы куплены";
            _hirePriceText.text = "-";
            _hireButton.interactable = false;
            return;
        }

        _hireTitleText.text = $"Нанять: {nextAdmin.DisplayName}";
        _hirePriceText.text = nextAdmin.HirePrice.ToString();
        _hireButton.interactable = true;
    }

    private void UpdateUpgradeCard(AdminWorker admin)
    {
        if (admin == null)
        {
            _upgradeTitleText.text = "Подойди к админу";
            _upgradePriceText.text = "-";
            _upgradeSpeedText.text = "-";
            _upgradeButton.interactable = false;
            return;
        }

        _upgradeTitleText.text = $"{admin.DisplayName} Ур. {admin.Level}";
        _upgradeSpeedText.text = $"Скорость: {admin.GetServiceInterval()} сек.";

        if (admin.CanUpgrade() == false)
        {
            _upgradePriceText.text = "MAX";
            _upgradeButton.interactable = false;
            return;
        }

        _upgradePriceText.text = admin.GetUpgradePrice().ToString();
        _upgradeButton.interactable = true;
    }
}