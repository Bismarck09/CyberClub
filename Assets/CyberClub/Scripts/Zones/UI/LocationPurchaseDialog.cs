using UnityEngine;
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
    private bool _interfaceWasActive;

    private void Awake()
    {
        ValidateReferences();
        HideImmediate();
    }

    private void OnEnable()
    {
        if (_view != null)
        {
            _view.BuyRequested += ConfirmPurchase;
            _view.CancelRequested += Close;
        }

        if (_paymentsService != null)
        {
            _paymentsService.OnProductLoadStateChanged += HandleProductLoadStateChanged;
            _paymentsService.OnPurchaseSuccess += HandlePremiumPurchaseSuccess;
            _paymentsService.OnPurchaseFailed += HandlePremiumPurchaseFailed;
        }
    }

    private void OnDisable()
    {
        if (_view != null)
        {
            _view.BuyRequested -= ConfirmPurchase;
            _view.CancelRequested -= Close;
        }

        if (_paymentsService != null)
        {
            _paymentsService.OnProductLoadStateChanged -= HandleProductLoadStateChanged;
            _paymentsService.OnPurchaseSuccess -= HandlePremiumPurchaseSuccess;
            _paymentsService.OnPurchaseFailed -= HandlePremiumPurchaseFailed;
        }
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
        {
            Debug.LogError(
                $"LocationPurchaseDialog: у ZonePurchaseConfig '{config.name}' " +
                "не настроена положительная вместимость.",
                config);
        }

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

        SetPremiumContent("Загрузка цены…");
        RefreshPremiumControls();
    }

    public void Close()
    {
        if (_paymentsService != null && _paymentsService.IsPurchasePending)
        {
            _view?.SetStatus(
                "Покупка обрабатывается. Дождитесь ответа платёжного окна.",
                true);
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

        if (_paymentsService.IsRetryAvailable)
        {
            _paymentsService.RetryCatalogLoad();
            RefreshPremiumControls();
            return;
        }

        if (_paymentsService.ProductLoadState == PremiumProductLoadState.EditorFallback)
        {
            _view?.SetStatus(
                "Тестовый режим: настоящая покупка доступна только в WebGL-версии Яндекс Игр.",
                false);
            return;
        }

        if (!_paymentsService.TryGetPremiumProduct(out _))
        {
            RefreshPremiumControls();
            return;
        }

        _paymentsService.BuyPremiumLocation();
        RefreshPremiumControls();
    }

    private void RefreshPremiumControls()
    {
        if (_mode != DialogMode.Premium || _view == null)
            return;

        if (_paymentsService == null)
        {
            _view.SetControls(false, true, "Недоступно");
            _view.SetStatus("Сервис платежей не назначен.", true);
            return;
        }

        if (_paymentsService.IsPurchasePending)
        {
            _view.SetControls(false, false, "Покупка…");
            _view.SetStatus(
                "Не закрывайте окно до завершения операции.",
                false);
            return;
        }

        switch (_paymentsService.ProductLoadState)
        {
            case PremiumProductLoadState.Loaded:
                ShowLoadedProduct(false);
                break;

            case PremiumProductLoadState.EditorFallback:
                ShowLoadedProduct(true);
                break;

            case PremiumProductLoadState.ProductNotFound:
                _view.SetControls(true, true, "Повторить");
                _view.SetStatus(
                    $"Товар '{_paymentsService.PremiumProductId}' не найден в каталоге Яндекс Игр.",
                    true);
                break;

            case PremiumProductLoadState.SdkUnavailable:
                _view.SetControls(true, true, "Повторить");
                _view.SetStatus(
                    "Магазин Яндекс Игр пока недоступен. Проверьте окружение запуска.",
                    true);
                break;

            case PremiumProductLoadState.Failed:
                _view.SetControls(true, true, "Повторить");
                _view.SetStatus(
                    string.IsNullOrWhiteSpace(_paymentsService.LastCatalogError)
                        ? "Не удалось загрузить цену."
                        : _paymentsService.LastCatalogError,
                    true);
                break;

            default:
                _view.SetControls(false, true, "Загрузка…");
                _view.SetStatus(
                    "Ожидаем каталог Яндекс Игр…",
                    false);
                break;
        }
    }

    private void ShowLoadedProduct(bool editorFallback)
    {
        if (!_paymentsService.TryGetPremiumProduct(out Purchase product))
        {
            _view.SetControls(false, true, "Недоступно");
            _view.SetStatus("Товар найден, но цена недоступна.", true);
            return;
        }

        SetPremiumContent(
            editorFallback
                ? $"{product.price} (UI preview)"
                : product.price);

        if (editorFallback)
        {
            _view.SetControls(false, true, "Тестовый режим");
            _view.SetStatus(
                "Editor fallback: цена служит только для проверки UI, покупка отключена.",
                false);
            return;
        }

        bool canBuy = _paymentsService.CanBuyPremium;
        _view.SetControls(canBuy, true, canBuy ? "Купить" : "Недоступно");
        _view.SetStatus(string.Empty, false);
    }

    private void HandleProductLoadStateChanged(PremiumProductLoadState unusedState)
    {
        RefreshPremiumControls();
    }

    private void HandlePremiumPurchaseSuccess(string purchaseId)
    {
        if (_paymentsService == null || purchaseId != _paymentsService.PremiumProductId)
            return;

        // Restoring a permanent purchase can happen while this dialog is hidden.
        // Do not change the current interface mode unless this dialog opened it.
        if (_mode == DialogMode.Premium)
            Close();
    }

    private void HandlePremiumPurchaseFailed(string purchaseId)
    {
        if (_paymentsService == null || purchaseId != _paymentsService.PremiumProductId)
            return;

        RefreshPremiumControls();
        _view?.SetStatus(
            "Покупка не завершена. Средства не списаны.",
            true);
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

    private void SetPremiumContent(string price)
    {
        int capacity = GetPremiumCapacity();

        if (capacity <= 0)
        {
            Debug.LogError(
                "LocationPurchaseDialog: у премиум-зоны не настроена положительная вместимость.",
                this);
        }

        _view?.SetContent(
            GetPremiumName(),
            GetPremiumDescription(),
            GetPremiumBenefit(),
            GetPremiumHint(),
            capacity > 0
                ? $"Игровых мест: {capacity}"
                : "Вместимость зоны не настроена",
            $"Цена: {price}",
            _premiumZoneConfig != null ? _premiumZoneConfig.PreviewSprite : null);
    }

    private void SetOrdinaryStatus(PurchaseFailureReason reason)
    {
        string message = reason switch
        {
            PurchaseFailureReason.NotEnoughCoins =>
                "Недостаточно монет — пополните баланс или вернитесь позже.",
            PurchaseFailureReason.FirstComputerRequired =>
                "Сначала купи первый компьютер",
            PurchaseFailureReason.TutorialStageIncomplete =>
                "Заверши текущий этап обучения",
            PurchaseFailureReason.TransactionFailed =>
                "Параметры зоны настроены неверно.",
            _ => string.Empty
        };

        _view?.SetStatus(message, reason != PurchaseFailureReason.None);
    }

    private string GetPremiumName()
    {
        return _premiumZoneInformation != null &&
            !string.IsNullOrWhiteSpace(_premiumZoneInformation.ZoneName)
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
        return _premiumZoneConfig != null
            ? _premiumZoneConfig.ProgressionHint
            : string.Empty;
    }

    private int GetPremiumCapacity()
    {
        if (_premiumZoneInformation != null && _premiumZoneInformation.SpawnPoints != null)
            return _premiumZoneInformation.SpawnPoints.AvailableSpawnPointCount;

        return _premiumZoneConfig != null
            ? _premiumZoneConfig.ComputerCapacity
            : 0;
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
        Debug.LogError(
            $"LocationPurchaseDialog: поле {fieldName} не назначено на GameObject '{name}'.",
            this);
    }
}
