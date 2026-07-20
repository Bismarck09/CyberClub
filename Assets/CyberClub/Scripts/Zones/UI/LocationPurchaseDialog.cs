using UnityEngine;
using YG;
using YG.Utils.Pay;

public class LocationPurchaseDialog : MonoBehaviour
{
    private enum DialogMode
    {
        None,
        Ordinary,
        Premium
    }

    [Header("Authored view")]
    [SerializeField] private ZonePurchasePopupView _view;

    [Header("Transactions")]
    [SerializeField] private ZonePurchase _zonePurchase;
    [SerializeField] private CyberClubYG2PaymentsService _paymentsService;
    [SerializeField] private PremiumLocationUnlocker _premiumUnlocker;
    [SerializeField] private PurchaseFeedbackPresenter _feedbackPresenter;

    [Header("Premium zone")]
    [SerializeField] private ZonePurchaseConfig _premiumZoneConfig;
    [SerializeField] private ZoneInformation _premiumZoneInformation;

    [Header("Input mode")]
    [SerializeField] private InteractionWithUI _interactionWithUI;

    private DialogMode _mode;
    private ZonePurchaseConfig _ordinaryConfig;
    private bool _catalogReceived;
    private bool _interfaceWasActive;

    private void Awake()
    {
        ValidateReferences();
        HideImmediate();
    }

    private void OnEnable()
    {
        YG2.onGetPayments += HandleCatalogReceived;

        if (_view != null)
        {
            _view.BuyRequested += ConfirmPurchase;
            _view.CancelRequested += Close;
        }

        if (_paymentsService != null)
        {
            _paymentsService.OnPurchaseSuccess += HandlePremiumPurchaseSuccess;
            _paymentsService.OnPurchaseFailed += HandlePremiumPurchaseFailed;
        }

        if (YG2.purchases != null && YG2.purchases.Length > 0)
            _catalogReceived = true;
    }

    private void OnDisable()
    {
        YG2.onGetPayments -= HandleCatalogReceived;

        if (_view != null)
        {
            _view.BuyRequested -= ConfirmPurchase;
            _view.CancelRequested -= Close;
        }

        if (_paymentsService != null)
        {
            _paymentsService.OnPurchaseSuccess -= HandlePremiumPurchaseSuccess;
            _paymentsService.OnPurchaseFailed -= HandlePremiumPurchaseFailed;
        }
    }

    private void Update()
    {
        if (_mode == DialogMode.Premium)
            RefreshPremiumControls();
    }

    public void OpenOrdinary(ZonePurchaseConfig config)
    {
        if (config == null || (_paymentsService != null && _paymentsService.IsPurchasePending))
            return;

        ShowRoot();
        _mode = DialogMode.Ordinary;
        _ordinaryConfig = config;

        int capacity = config.ComputerCapacity;
        string capacityText = capacity > 0
            ? $"Игровых мест: {capacity}"
            : "Вместимость зоны не настроена";

        if (capacity <= 0)
            Debug.LogError($"LocationPurchaseDialog: у ZonePurchaseConfig '{config.name}' не настроена положительная вместимость.", config);

        _view?.SetContent(
            config.DisplayName,
            config.Description,
            $"Польза: {config.GameplayBenefit}",
            config.ProgressionHint,
            capacityText,
            config.ZonePrice == 0
                ? "Цена: Бесплатно"
                : $"Цена: {ResourceValueFormatter.Format(config.ZonePrice)} монет",
            config.PreviewSprite);

        PurchaseFailureReason reason = _zonePurchase != null
            ? _zonePurchase.GetFailureReason(config)
            : PurchaseFailureReason.TransactionFailed;

        bool canExplainByClick = reason == PurchaseFailureReason.None ||
            reason == PurchaseFailureReason.NotEnoughCoins ||
            reason == PurchaseFailureReason.FirstComputerRequired ||
            reason == PurchaseFailureReason.TutorialStageIncomplete;

        _view?.SetControls(canExplainByClick, true, "Купить");
        SetOrdinaryStatus(reason);
    }

    public void OpenPremium()
    {
        if (_premiumUnlocker != null && _premiumUnlocker.IsUnlocked)
        {
            _feedbackPresenter?.Show(PurchaseFailureReason.ProductUnavailable);
            return;
        }

        ShowRoot();
        _mode = DialogMode.Premium;
        _ordinaryConfig = null;

        string zoneName = _premiumZoneInformation != null &&
            !string.IsNullOrWhiteSpace(_premiumZoneInformation.ZoneName)
            ? _premiumZoneInformation.ZoneName
            : "Премиум-зона";
        int capacity = _premiumZoneInformation != null && _premiumZoneInformation.SpawnPoints != null
            ? _premiumZoneInformation.SpawnPoints.AvailableSpawnPointCount
            : (_premiumZoneConfig != null ? _premiumZoneConfig.ComputerCapacity : 0);

        if (capacity <= 0)
            Debug.LogError("LocationPurchaseDialog: у премиум-зоны не настроена положительная вместимость.", this);

        _view?.SetContent(
            zoneName,
            _premiumZoneConfig != null
                ? _premiumZoneConfig.Description
                : "Покупка навсегда открывает премиум-зону клуба и её игровые места.",
            _premiumZoneConfig != null
                ? $"Польза: {_premiumZoneConfig.GameplayBenefit}"
                : "Польза: отдельная зона с премиум-посетителями.",
            _premiumZoneConfig != null ? _premiumZoneConfig.ProgressionHint : string.Empty,
            capacity > 0 ? $"Игровых мест: {capacity}" : "Вместимость зоны не настроена",
            "Загрузка цены…",
            _premiumZoneConfig != null ? _premiumZoneConfig.PreviewSprite : null);

        RefreshPremiumControls();
    }

    public void Close()
    {
        if (_paymentsService != null && _paymentsService.IsPurchasePending)
        {
            _view?.SetStatus("Покупка обрабатывается. Дождитесь ответа платёжного окна.", true);
            return;
        }

        HideImmediate();
        _interactionWithUI?.SetInteracts(_interfaceWasActive);
    }

    private void ConfirmPurchase()
    {
        if (_mode == DialogMode.Ordinary)
        {
            if (_zonePurchase != null && _zonePurchase.TryBuyConfirmed(_ordinaryConfig))
                Close();
            else
                OpenOrdinary(_ordinaryConfig);

            return;
        }

        if (_mode != DialogMode.Premium || _paymentsService == null)
            return;

        if (!TryGetPremiumProduct(out _))
        {
            _view?.SetStatus(
                _catalogReceived
                    ? "Цена недоступна. Повторите попытку позже."
                    : "Загрузка цены…",
                _catalogReceived);
            return;
        }

        _paymentsService.BuyPremiumLocation();
        RefreshPremiumControls();
    }

    private void RefreshPremiumControls()
    {
        if (_mode != DialogMode.Premium || _view == null)
            return;

        bool pending = _paymentsService != null && _paymentsService.IsPurchasePending;

        if (pending)
        {
            _view.SetControls(false, false, "Покупка…");
            _view.SetStatus("Не закрывайте окно до завершения операции.", false);
            return;
        }

        if (TryGetPremiumProduct(out Purchase product))
        {
            _view.SetContent(
                GetPremiumName(),
                GetPremiumDescription(),
                GetPremiumBenefit(),
                GetPremiumHint(),
                GetPremiumCapacityText(),
                $"Цена: {product.price}",
                _premiumZoneConfig != null ? _premiumZoneConfig.PreviewSprite : null);
            _view.SetControls(_premiumUnlocker == null || !_premiumUnlocker.IsUnlocked, true, "Купить");
            _view.SetStatus(string.Empty, false);
            return;
        }

        _view.SetControls(false, true, "Купить");
        _view.SetStatus(
            _catalogReceived
                ? "Не удалось получить товар из каталога Яндекс Игр."
                : "Ожидаем данные каталога Яндекс Игр.",
            _catalogReceived);
    }

    private bool TryGetPremiumProduct(out Purchase product)
    {
        product = null;

        if (_paymentsService == null || string.IsNullOrWhiteSpace(_paymentsService.PremiumProductId))
            return false;

        product = YG2.PurchaseByID(_paymentsService.PremiumProductId);
        return product != null && !string.IsNullOrWhiteSpace(product.price);
    }

    private void HandleCatalogReceived()
    {
        _catalogReceived = true;
        RefreshPremiumControls();
    }

    private void HandlePremiumPurchaseSuccess(string purchaseId)
    {
        if (_paymentsService == null || purchaseId != _paymentsService.PremiumProductId)
            return;

        Close();
    }

    private void HandlePremiumPurchaseFailed(string purchaseId)
    {
        if (_paymentsService == null || purchaseId != _paymentsService.PremiumProductId)
            return;

        RefreshPremiumControls();
        _view?.SetStatus("Покупка не завершена. Средства не списаны.", true);
    }

    private void ShowRoot()
    {
        if (_view == null)
        {
            ReportMissing(nameof(_view));
            return;
        }

        _interfaceWasActive = _interactionWithUI != null && _interactionWithUI.IsInteracts;
        _interactionWithUI?.SetInteracts(true);
        _view.Show();
    }

    private void HideImmediate()
    {
        _mode = DialogMode.None;
        _ordinaryConfig = null;
        _view?.Hide();
    }

    private void SetOrdinaryStatus(PurchaseFailureReason reason)
    {
        string message = reason switch
        {
            PurchaseFailureReason.NotEnoughCoins => "Недостаточно монет — пополните баланс или вернитесь позже.",
            PurchaseFailureReason.FirstComputerRequired => "Сначала купи первый компьютер",
            PurchaseFailureReason.TutorialStageIncomplete => "Заверши текущий этап обучения",
            PurchaseFailureReason.TransactionFailed => "Параметры зоны настроены неверно.",
            _ => string.Empty
        };

        _view?.SetStatus(message, reason != PurchaseFailureReason.None);
    }

    private string GetPremiumName()
    {
        return _premiumZoneInformation != null && !string.IsNullOrWhiteSpace(_premiumZoneInformation.ZoneName)
            ? _premiumZoneInformation.ZoneName
            : "Премиум-зона";
    }

    private string GetPremiumDescription()
    {
        return _premiumZoneConfig != null
            ? _premiumZoneConfig.Description
            : "Покупка навсегда открывает премиум-зону клуба и её игровые места.";
    }

    private string GetPremiumBenefit()
    {
        return _premiumZoneConfig != null
            ? $"Польза: {_premiumZoneConfig.GameplayBenefit}"
            : "Польза: отдельная зона с премиум-посетителями.";
    }

    private string GetPremiumHint()
    {
        return _premiumZoneConfig != null ? _premiumZoneConfig.ProgressionHint : string.Empty;
    }

    private string GetPremiumCapacityText()
    {
        int capacity = _premiumZoneInformation != null && _premiumZoneInformation.SpawnPoints != null
            ? _premiumZoneInformation.SpawnPoints.AvailableSpawnPointCount
            : (_premiumZoneConfig != null ? _premiumZoneConfig.ComputerCapacity : 0);
        return capacity > 0 ? $"Игровых мест: {capacity}" : "Вместимость зоны не настроена";
    }

    private void ValidateReferences()
    {
        if (_view == null)
            ReportMissing(nameof(_view));
        if (_zonePurchase == null)
            ReportMissing(nameof(_zonePurchase));
        if (_paymentsService == null)
            ReportMissing(nameof(_paymentsService));
        if (_premiumUnlocker == null)
            ReportMissing(nameof(_premiumUnlocker));
        if (_feedbackPresenter == null)
            ReportMissing(nameof(_feedbackPresenter));
        if (_premiumZoneConfig == null)
            ReportMissing(nameof(_premiumZoneConfig));
        if (_premiumZoneInformation == null)
            ReportMissing(nameof(_premiumZoneInformation));
        if (_interactionWithUI == null)
            ReportMissing(nameof(_interactionWithUI));
    }

    private void ReportMissing(string fieldName)
    {
        Debug.LogError($"LocationPurchaseDialog: поле {fieldName} не назначено на GameObject '{name}'.", this);
    }
}
