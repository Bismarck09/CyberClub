using TMPro;
using UnityEngine;
using UnityEngine.UI;
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

    private GameObject _root;
    private RectTransform _safeAreaRoot;
    private TMP_Text _titleText;
    private TMP_Text _bodyText;
    private TMP_Text _advantagesText;
    private TMP_Text _capacityText;
    private TMP_Text _priceText;
    private TMP_Text _statusText;
    private Button _buyButton;
    private TMP_Text _buyButtonText;
    private Button _cancelButton;

    private DialogMode _mode;
    private ZonePurchaseConfig _ordinaryConfig;
    private Rect _lastSafeArea;
    private bool _catalogReceived;
    private bool _interfaceWasActive;

    private void Awake()
    {
        BuildRuntimeView();
        ApplySafeArea();
        HideImmediate();
    }

    private void OnEnable()
    {
        YG2.onGetPayments += HandleCatalogReceived;

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

        if (_paymentsService != null)
        {
            _paymentsService.OnPurchaseSuccess -= HandlePremiumPurchaseSuccess;
            _paymentsService.OnPurchaseFailed -= HandlePremiumPurchaseFailed;
        }
    }

    private void Update()
    {
        if (_lastSafeArea != Screen.safeArea)
            ApplySafeArea();

        if (_mode == DialogMode.Premium && _root != null && _root.activeSelf)
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

        _titleText.text = config.DisplayName;
        _bodyText.text = config.Description;
        _advantagesText.text = $"Преимущества: {config.Advantages}";
        _capacityText.text = capacity > 0
            ? $"Игровых мест: {capacity}"
            : "Игровые места откроются вместе с зоной";
        _priceText.text = $"Цена: {ResourceValueFormatter.Format(config.ZonePrice)} монет";
        _buyButtonText.text = "Купить";
        _cancelButton.interactable = true;

        PurchaseFailureReason reason = _zonePurchase != null
            ? _zonePurchase.GetFailureReason(config)
            : PurchaseFailureReason.TransactionFailed;

        _buyButton.interactable = reason == PurchaseFailureReason.None ||
            reason == PurchaseFailureReason.NotEnoughCoins;
        SetStatus(reason == PurchaseFailureReason.NotEnoughCoins
            ? "Недостаточно монет — пополните баланс или вернитесь позже."
            : string.Empty,
            reason == PurchaseFailureReason.NotEnoughCoins);
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
        int gems = _paymentsService != null ? _paymentsService.PremiumGemsReward : 0;

        _titleText.text = zoneName;
        _bodyText.text = "Покупка навсегда открывает премиум-зону клуба и её игровые места.";
        _advantagesText.text = $"Преимущества: отдельная зона, премиум-посетители и бонус {gems} кристаллов.";
        _capacityText.text = capacity > 0 ? $"Игровых мест: {capacity}" : "Игровые места: данные зоны";
        _buyButtonText.text = "Купить";
        RefreshPremiumControls();
    }

    public void Close()
    {
        if (_paymentsService != null && _paymentsService.IsPurchasePending)
        {
            SetStatus("Покупка обрабатывается. Дождитесь ответа платёжного окна.", true);
            return;
        }

        HideImmediate();

        if (_interactionWithUI != null)
            _interactionWithUI.SetInteracts(_interfaceWasActive);
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
            SetStatus(_catalogReceived
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
        if (_mode != DialogMode.Premium || _buyButton == null)
            return;

        bool pending = _paymentsService != null && _paymentsService.IsPurchasePending;
        _cancelButton.interactable = !pending;

        if (pending)
        {
            _buyButton.interactable = false;
            _priceText.text = "Покупка обрабатывается…";
            SetStatus("Не закрывайте окно до завершения операции.", false);
            return;
        }

        if (TryGetPremiumProduct(out Purchase product))
        {
            _priceText.text = $"Цена: {product.price}";
            _buyButton.interactable = _premiumUnlocker == null || !_premiumUnlocker.IsUnlocked;
            SetStatus(string.Empty, false);
            return;
        }

        _buyButton.interactable = false;

        if (_catalogReceived)
        {
            _priceText.text = "Цена недоступна";
            SetStatus("Не удалось получить товар из каталога Яндекс Игр.", true);
        }
        else
        {
            _priceText.text = "Загрузка цены…";
            SetStatus("Ожидаем данные каталога Яндекс Игр.", false);
        }
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
        SetStatus("Покупка не завершена. Средства не списаны.", true);
    }

    private void ShowRoot()
    {
        if (_root == null)
            BuildRuntimeView();

        if (!_root.activeSelf)
        {
            _interfaceWasActive = _interactionWithUI != null && _interactionWithUI.IsInteracts;
            _interactionWithUI?.SetInteracts(true);
        }

        _root.SetActive(true);
    }

    private void HideImmediate()
    {
        _mode = DialogMode.None;
        _ordinaryConfig = null;

        if (_root != null)
            _root.SetActive(false);
    }

    private void SetStatus(string message, bool isError)
    {
        if (_statusText == null)
            return;

        _statusText.text = message;
        _statusText.color = isError ? new Color(1f, 0.43f, 0.4f) : new Color(0.78f, 0.86f, 1f);
    }

    private void BuildRuntimeView()
    {
        if (_root != null)
            return;

        _root = CreateUiObject("LocationPurchaseDialog", transform);
        RectTransform rootRect = (RectTransform)_root.transform;
        Stretch(rootRect);
        Image overlay = _root.AddComponent<Image>();
        overlay.color = new Color(0.015f, 0.02f, 0.035f, 0.82f);
        overlay.raycastTarget = true;

        GameObject safeObject = CreateUiObject("SafeArea", rootRect);
        _safeAreaRoot = (RectTransform)safeObject.transform;

        GameObject panelObject = CreateUiObject("Card", _safeAreaRoot);
        RectTransform panel = (RectTransform)panelObject.transform;
        panel.anchorMin = new Vector2(0.14f, 0.08f);
        panel.anchorMax = new Vector2(0.86f, 0.92f);
        panel.offsetMin = Vector2.zero;
        panel.offsetMax = Vector2.zero;
        Image card = panelObject.AddComponent<Image>();
        card.color = new Color(0.065f, 0.085f, 0.14f, 0.98f);

        _titleText = CreateText("Title", panel, new Vector2(0.07f, 0.82f), new Vector2(0.93f, 0.96f), 42f, FontStyles.Bold);
        _bodyText = CreateText("Description", panel, new Vector2(0.07f, 0.59f), new Vector2(0.93f, 0.81f), 28f, FontStyles.Normal);
        _advantagesText = CreateText("Advantages", panel, new Vector2(0.07f, 0.42f), new Vector2(0.93f, 0.59f), 26f, FontStyles.Normal);
        _capacityText = CreateText("Capacity", panel, new Vector2(0.07f, 0.33f), new Vector2(0.93f, 0.43f), 26f, FontStyles.Bold);
        _priceText = CreateText("Price", panel, new Vector2(0.07f, 0.23f), new Vector2(0.93f, 0.34f), 30f, FontStyles.Bold);
        _statusText = CreateText("Status", panel, new Vector2(0.07f, 0.14f), new Vector2(0.93f, 0.24f), 23f, FontStyles.Normal);

        _buyButton = CreateButton("BuyButton", panel, new Vector2(0.52f, 0.035f), new Vector2(0.93f, 0.14f), new Color(0.08f, 0.58f, 0.82f), out _buyButtonText);
        _cancelButton = CreateButton("CancelButton", panel, new Vector2(0.07f, 0.035f), new Vector2(0.48f, 0.14f), new Color(0.23f, 0.27f, 0.36f), out TMP_Text cancelText);
        cancelText.text = "Отмена";
        _buyButton.onClick.AddListener(ConfirmPurchase);
        _cancelButton.onClick.AddListener(Close);
    }

    private void ApplySafeArea()
    {
        if (_safeAreaRoot == null || Screen.width <= 0 || Screen.height <= 0)
            return;

        Rect safeArea = Screen.safeArea;
        _lastSafeArea = safeArea;
        _safeAreaRoot.anchorMin = new Vector2(safeArea.xMin / Screen.width, safeArea.yMin / Screen.height);
        _safeAreaRoot.anchorMax = new Vector2(safeArea.xMax / Screen.width, safeArea.yMax / Screen.height);
        _safeAreaRoot.offsetMin = Vector2.zero;
        _safeAreaRoot.offsetMax = Vector2.zero;
    }

    private static GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.layer = parent.gameObject.layer;
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static TMP_Text CreateText(
        string name,
        RectTransform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        float fontSize,
        FontStyles style)
    {
        GameObject gameObject = CreateUiObject(name, parent);
        RectTransform rect = (RectTransform)gameObject.transform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = gameObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.enableAutoSizing = true;
        text.fontSizeMin = 15f;
        text.fontSizeMax = fontSize;
        text.fontStyle = style;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(
        string name,
        RectTransform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color color,
        out TMP_Text label)
    {
        GameObject gameObject = CreateUiObject(name, parent);
        RectTransform rect = (RectTransform)gameObject.transform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = gameObject.AddComponent<Image>();
        image.color = color;
        Button button = gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        label = CreateText("Label", rect, Vector2.zero, Vector2.one, 28f, FontStyles.Bold);
        label.alignment = TextAlignmentOptions.Center;
        return button;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
