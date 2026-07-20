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
    [SerializeField] private TutorialPurchaseGate _tutorialPurchaseGate;

    [Header("Tutorial blocked states")]
    [SerializeField] private GameObject _hireTutorialBlockedState;
    [SerializeField] private TextMeshProUGUI _hireTutorialBlockedText;
    [SerializeField] private GameObject _upgradeTutorialBlockedState;
    [SerializeField] private TextMeshProUGUI _upgradeTutorialBlockedText;

    private void OnEnable()
    {
        if (_zoneSwitcher != null)
        {
            _zoneSwitcher.OnZoneChanged += OnZoneChanged;
            _zoneSwitcher.OnZoneExited += OnZoneExited;
        }

        if (_adminPurchase != null)
            _adminPurchase.OnAdminStateChanged += UpdateHireCard;

        if (_adminUpgradePurchase != null)
        {
            _adminUpgradePurchase.OnSelectedAdminChanged += UpdateUpgradeCard;
            _adminUpgradePurchase.OnAdminUpgraded += UpdateUpgradeCard;
        }

        if (_tutorialPurchaseGate != null)
            _tutorialPurchaseGate.OnGateStateChanged += UpdateTutorialBlockedStates;

        UpdateHireCard();
        UpdateUpgradeCard(null);
        UpdateTutorialBlockedStates();
    }

    private void OnDisable()
    {
        if (_zoneSwitcher != null)
        {
            _zoneSwitcher.OnZoneChanged -= OnZoneChanged;
            _zoneSwitcher.OnZoneExited -= OnZoneExited;
        }

        if (_adminPurchase != null)
            _adminPurchase.OnAdminStateChanged -= UpdateHireCard;

        if (_adminUpgradePurchase != null)
        {
            _adminUpgradePurchase.OnSelectedAdminChanged -= UpdateUpgradeCard;
            _adminUpgradePurchase.OnAdminUpgraded -= UpdateUpgradeCard;
        }

        if (_tutorialPurchaseGate != null)
            _tutorialPurchaseGate.OnGateStateChanged -= UpdateTutorialBlockedStates;
    }

    private void OnZoneChanged(ZoneInformation zoneInformation)
    {
        UpdateHireCard();
        UpdateUpgradeCard(_adminUpgradePurchase != null ? _adminUpgradePurchase.SelectedAdmin : null);
    }

    private void OnZoneExited()
    {
        if (_upgradeButton != null)
            _upgradeButton.interactable = false;
    }

    private void UpdateHireCard()
    {
        if (_adminPurchase == null)
            return;

        AdminWorker nextAdmin = _adminPurchase.GetNextNotHiredAdmin();

        if (nextAdmin == null)
        {
            _hireTitleText.text = "Все админы куплены";
            _hirePriceText.text = "-";
            if (_hireButton != null)
                _hireButton.interactable = false;
            return;
        }

        _hireTitleText.text = $"Нанять: {nextAdmin.DisplayName}";
        _hirePriceText.text = ResourceValueFormatter.Format(nextAdmin.HirePrice);

        if (_adminPurchase.IsHiringLockedByTutorial)
            _hireTitleText.text = "Доступно после первого дохода";

        if (_hireButton != null)
            _hireButton.interactable = true;
    }

    private void UpdateUpgradeCard(AdminWorker admin)
    {
        if (admin == null)
        {
            _upgradeTitleText.text = "Подойди к админу";
            _upgradePriceText.text = "-";
            _upgradeSpeedText.text = "-";
            if (_upgradeButton != null)
                _upgradeButton.interactable = false;
            return;
        }

        _upgradeTitleText.text = $"{admin.DisplayName} Ур. {admin.Level}";
        _upgradeSpeedText.text = $"Скорость: {admin.GetServiceInterval()} сек.";

        if (admin.CanUpgrade() == false)
        {
            _upgradePriceText.text = "MAX";
            if (_upgradeButton != null)
                _upgradeButton.interactable = false;
            return;
        }

        _upgradePriceText.text = ResourceValueFormatter.Format(admin.GetUpgradePrice());

        if (_upgradeButton != null)
            _upgradeButton.interactable = _adminUpgradePurchase != null;
    }

    private void UpdateTutorialBlockedStates()
    {
        SetTutorialBlockedState(
            TutorialPurchaseCategory.AdminHire,
            _hireTutorialBlockedState,
            _hireTutorialBlockedText);
        SetTutorialBlockedState(
            TutorialPurchaseCategory.AdminUpgrade,
            _upgradeTutorialBlockedState,
            _upgradeTutorialBlockedText);
    }

    private void SetTutorialBlockedState(
        TutorialPurchaseCategory category,
        GameObject root,
        TextMeshProUGUI text)
    {
        PurchaseFailureReason reason = PurchaseFailureReason.TransactionFailed;
        bool blocked = _tutorialPurchaseGate == null ||
            !_tutorialPurchaseGate.CanPurchase(category, out reason);

        if (root != null)
            root.SetActive(blocked);

        if (text != null)
            text.text = blocked ? PurchaseFailureMessage.Get(reason) : string.Empty;
    }
}
