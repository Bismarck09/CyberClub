using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using YG;
using YG.Utils.Pay;

public enum PremiumProductLoadState
{
    NotStarted,
    Loading,
    Loaded,
    ProductNotFound,
    SdkUnavailable,
    Failed,
    EditorFallback
}

[DisallowMultipleComponent]
public class CyberClubYG2PaymentsService : MonoBehaviour
{
    [Header("Yandex product")]
    [SerializeField] private string _premiumZoneProductId = "premium_zone_100";
    [SerializeField, Range(10f, 15f)] private float _catalogTimeoutSeconds = 12f;

    [Header("Editor-only UI preview")]
    [SerializeField] private string _editorPreviewPrice = "100 YAN";

    [Header("Premium")]
    [SerializeField] private PremiumLocationUnlocker _premiumLocationUnlocker;

    [Header("Save")]
    [SerializeField] private SaveLoadManager _saveLoadManager;
    [SerializeField] private PurchaseFeedbackPresenter _feedbackPresenter;

    public event Action<string> OnPurchaseSuccess;
    public event Action<string> OnPurchaseFailed;
    public event Action<PremiumProductLoadState> OnProductLoadStateChanged;

    private bool _purchaseRequestPending;
    private bool _catalogRequestStarted;
    private bool _isHandlingSuccess;
    private bool _saveIsReady;
    private bool _restoredNonConsumableThisSession;
    private Coroutine _catalogTimeoutRoutine;
    private Purchase _editorPreviewProduct;

    public string PremiumProductId => _premiumZoneProductId;
    public bool IsPurchasePending => _purchaseRequestPending;
    public PremiumProductLoadState ProductLoadState { get; private set; } =
        PremiumProductLoadState.NotStarted;
    public string LastCatalogError { get; private set; } = string.Empty;
    public bool IsRetryAvailable =>
        ProductLoadState == PremiumProductLoadState.ProductNotFound ||
        ProductLoadState == PremiumProductLoadState.SdkUnavailable ||
        ProductLoadState == PremiumProductLoadState.Failed;
    public bool CanBuyPremium =>
        ProductLoadState == PremiumProductLoadState.Loaded &&
        !_purchaseRequestPending &&
        _saveIsReady &&
        _premiumLocationUnlocker != null &&
        !_premiumLocationUnlocker.IsUnlocked;

#if UNITY_WEBGL && !UNITY_EDITOR && YandexGamesPlatform_yg
    [DllImport("__Internal")]
    private static extern int CyberClub_RetryPaymentsCatalog_js();

    [DllImport("__Internal")]
    private static extern int CyberClub_BuyNonConsumable_js(string productId);
#endif

    private void OnEnable()
    {
        YG2.onGetSDKData += HandleSdkReady;
        YG2.onGetPayments += HandleCatalogReceived;
        YG2.onPurchaseSuccess += HandlePurchaseSuccess;
        YG2.onPurchaseFailed += HandlePurchaseFailed;
    }

    private void Start()
    {
        ValidateReferences();

        if (_saveLoadManager != null)
        {
            if (_saveLoadManager.IsLoaded)
                HandleSaveLoaded();
            else
                _saveLoadManager.OnGameLoaded += HandleSaveLoaded;
        }

        BeginCatalogLoad(true);
    }

    private void OnDisable()
    {
        YG2.onGetSDKData -= HandleSdkReady;
        YG2.onGetPayments -= HandleCatalogReceived;
        YG2.onPurchaseSuccess -= HandlePurchaseSuccess;
        YG2.onPurchaseFailed -= HandlePurchaseFailed;

        if (_saveLoadManager != null)
            _saveLoadManager.OnGameLoaded -= HandleSaveLoaded;

        StopCatalogTimeout();
    }

    public bool TryGetPremiumProduct(out Purchase product)
    {
        if (ProductLoadState == PremiumProductLoadState.EditorFallback)
        {
            product = _editorPreviewProduct;
            return product != null;
        }

        product = FindProductInCatalog();
        return product != null && !string.IsNullOrWhiteSpace(product.price);
    }

    public void RetryCatalogLoad()
    {
        if (_purchaseRequestPending)
            return;

        Debug.Log(
            $"CyberClubYG2PaymentsService: повторный запрос каталога. " +
            $"Product ID: '{_premiumZoneProductId}'.",
            this);
        BeginCatalogLoad(false);
    }

    public void BuyPremiumLocation()
    {
        if (ProductLoadState == PremiumProductLoadState.EditorFallback)
        {
            Debug.LogWarning(
                "CyberClubYG2PaymentsService: Editor fallback предназначен только для проверки UI. " +
                "Реальная покупка не запускалась.",
                this);
            OnPurchaseFailed?.Invoke(_premiumZoneProductId);
            return;
        }

        if (!CanBuyPremium || !TryGetPremiumProduct(out _))
        {
            if (_premiumLocationUnlocker != null && _premiumLocationUnlocker.IsUnlocked)
                _feedbackPresenter?.Show(PurchaseFailureReason.ProductUnavailable);
            else if (!_purchaseRequestPending)
                _feedbackPresenter?.Show(PurchaseFailureReason.TransactionFailed);

            return;
        }

        _purchaseRequestPending = true;
        Debug.Log(
            $"CyberClubYG2PaymentsService: запуск покупки постоянного товара " +
            $"'{_premiumZoneProductId}'.",
            this);

        try
        {
#if UNITY_WEBGL && !UNITY_EDITOR && YandexGamesPlatform_yg
            YG2.PauseGame(true);

            if (CyberClub_BuyNonConsumable_js(_premiumZoneProductId) == 0)
            {
                YG2.PauseGame(false);
                FailPurchaseRequest("WebGL-мост платежей недоступен.");
            }
#else
            FailPurchaseRequest(
                "Покупки доступны только в WebGL-сборке для платформы Яндекс Игры.");
#endif
        }
        catch (Exception exception)
        {
            _purchaseRequestPending = false;
            YG2.PauseGame(false);
            Debug.LogException(exception, this);
            _feedbackPresenter?.Show(PurchaseFailureReason.TransactionFailed);
            OnPurchaseFailed?.Invoke(_premiumZoneProductId);
        }
    }

    private void BeginCatalogLoad(bool allowCachedCatalog)
    {
        StopCatalogTimeout();
        _catalogRequestStarted = false;
        LastCatalogError = string.Empty;

        if (string.IsNullOrWhiteSpace(_premiumZoneProductId))
        {
            SetLoadFailure(
                PremiumProductLoadState.ProductNotFound,
                "Product ID премиум-зоны не задан в Inspector.");
            return;
        }

#if UNITY_EDITOR
        _editorPreviewProduct = new Purchase
        {
            id = _premiumZoneProductId,
            title = "Premium Zone (Editor Preview)",
            description = "Визуальный fallback без реальной покупки.",
            price = string.IsNullOrWhiteSpace(_editorPreviewPrice)
                ? "TEST"
                : _editorPreviewPrice,
            priceValue = string.Empty,
            priceCurrencyCode = string.Empty,
            consumed = true
        };

        SetLoadState(PremiumProductLoadState.EditorFallback);
        Debug.Log(
            $"CyberClubYG2PaymentsService: включён Editor UI fallback для " +
            $"'{_premiumZoneProductId}'. Реальная покупка отключена.",
            this);
        return;
#else
        if (allowCachedCatalog && TryResolveAlreadyLoadedCatalog())
            return;

        SetLoadState(PremiumProductLoadState.Loading);
        _catalogTimeoutRoutine = StartCoroutine(CatalogTimeoutRoutine());

        if (!YG2.isSDKEnabled)
        {
            Debug.Log(
                "CyberClubYG2PaymentsService: ожидается готовность YG2 SDK перед запросом каталога.",
                this);
            return;
        }

        RequestCatalogFromPlatform();
#endif
    }

    private void HandleSdkReady()
    {
        Debug.Log(
            $"CyberClubYG2PaymentsService: YG2 SDK ready={YG2.isSDKEnabled}.",
            this);

        if (ProductLoadState == PremiumProductLoadState.Loading)
            RequestCatalogFromPlatform();
    }

    private void RequestCatalogFromPlatform()
    {
        if (_catalogRequestStarted ||
            ProductLoadState != PremiumProductLoadState.Loading)
        {
            return;
        }

        _catalogRequestStarted = true;
        Debug.Log(
            $"CyberClubYG2PaymentsService: запрос каталога начат; искомый Product ID " +
            $"'{_premiumZoneProductId}'.",
            this);

#if UNITY_WEBGL && !UNITY_EDITOR && YandexGamesPlatform_yg
        try
        {
            if (CyberClub_RetryPaymentsCatalog_js() == 0)
            {
                SetLoadFailure(
                    PremiumProductLoadState.SdkUnavailable,
                    "JS API платежей Яндекс Игр не найден в текущем окружении.");
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
            SetLoadFailure(
                PremiumProductLoadState.Failed,
                $"Ошибка запуска запроса каталога: {exception.Message}");
        }
#else
        SetLoadFailure(
            PremiumProductLoadState.SdkUnavailable,
            "Каталог платежей доступен только в WebGL-сборке Яндекс Игр.");
#endif
    }

    private void HandleCatalogReceived()
    {
        _catalogRequestStarted = false;
        int count = YG2.purchases?.Length ?? 0;
        Debug.Log(
            $"CyberClubYG2PaymentsService: каталог получен, товаров: {count}; " +
            $"искомый Product ID: '{_premiumZoneProductId}'.",
            this);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        Debug.Log(
            $"CyberClubYG2PaymentsService: ID каталога: {GetCatalogIdsForLog()}.",
            this);
#endif

        ResolveReceivedCatalog();
    }

    private bool TryResolveAlreadyLoadedCatalog()
    {
        if (YG2.purchases == null || YG2.purchases.Length == 0)
            return false;

        Debug.Log(
            "CyberClubYG2PaymentsService: используется каталог, загруженный до активации сцены.",
            this);
        ResolveReceivedCatalog();
        return true;
    }

    private void ResolveReceivedCatalog()
    {
        StopCatalogTimeout();

        int count = YG2.purchases?.Length ?? 0;
        if (count == 0)
        {
            SetLoadFailure(
                PremiumProductLoadState.Failed,
                "Каталог Яндекс Игр получен пустым.");
            return;
        }

        Purchase product = FindProductInCatalog();
        if (product == null)
        {
            SetLoadFailure(
                PremiumProductLoadState.ProductNotFound,
                $"Товар '{_premiumZoneProductId}' не найден в каталоге Яндекс Игр.");
            return;
        }

        if (string.IsNullOrWhiteSpace(product.price))
        {
            SetLoadFailure(
                PremiumProductLoadState.Failed,
                $"Товар '{_premiumZoneProductId}' найден, но SDK вернул пустую цену.");
            return;
        }

        SetLoadState(PremiumProductLoadState.Loaded);
        Debug.Log(
            $"CyberClubYG2PaymentsService: товар '{_premiumZoneProductId}' найден; " +
            $"цена SDK: '{product.price}', purchased={!product.consumed}.",
            this);
        TryRestoreNonConsumablePurchase();
    }

    private Purchase FindProductInCatalog()
    {
        if (YG2.purchases == null)
            return null;

        for (int i = 0; i < YG2.purchases.Length; i++)
        {
            Purchase product = YG2.purchases[i];
            if (product != null && product.id == _premiumZoneProductId)
                return product;
        }

        return null;
    }

    private void HandleSaveLoaded()
    {
        if (_saveLoadManager != null)
            _saveLoadManager.OnGameLoaded -= HandleSaveLoaded;

        _saveIsReady = true;
        TryRestoreNonConsumablePurchase();
    }

    private void TryRestoreNonConsumablePurchase()
    {
        if (_restoredNonConsumableThisSession ||
            !_saveIsReady ||
            ProductLoadState != PremiumProductLoadState.Loaded)
        {
            return;
        }

        Purchase product = FindProductInCatalog();
        if (product == null || product.consumed)
            return;

        _restoredNonConsumableThisSession = true;
        Debug.Log(
            $"CyberClubYG2PaymentsService: восстановлена постоянная покупка " +
            $"'{_premiumZoneProductId}' из getPurchases; запись не консумируется.",
            this);
        GrantPremiumEntitlement(_premiumZoneProductId, true);
    }

    private void HandlePurchaseSuccess(string purchaseId)
    {
        if (purchaseId != _premiumZoneProductId)
            return;

        _purchaseRequestPending = false;
        Debug.Log(
            $"CyberClubYG2PaymentsService: успешный callback покупки '{purchaseId}'.",
            this);
        GrantPremiumEntitlement(purchaseId, false);
    }

    private void GrantPremiumEntitlement(string purchaseId, bool restored)
    {
        if (_isHandlingSuccess ||
            _premiumLocationUnlocker == null ||
            _saveLoadManager == null ||
            !_saveLoadManager.IsLoaded)
        {
            return;
        }

        _isHandlingSuccess = true;

        try
        {
            _premiumLocationUnlocker.UnlockPremiumLocation();
            _saveLoadManager.SaveGame();
            OnPurchaseSuccess?.Invoke(purchaseId);

            Debug.Log(
                restored
                    ? $"CyberClubYG2PaymentsService: право '{purchaseId}' восстановлено и сохранено."
                    : $"CyberClubYG2PaymentsService: право '{purchaseId}' выдано и сохранено.",
                this);
        }
        finally
        {
            _isHandlingSuccess = false;
        }
    }

    private void HandlePurchaseFailed(string purchaseId)
    {
        if (purchaseId != _premiumZoneProductId)
            return;

        _purchaseRequestPending = false;
        OnPurchaseFailed?.Invoke(purchaseId);
        _feedbackPresenter?.Show(PurchaseFailureReason.TransactionFailed);
        Debug.LogWarning(
            $"CyberClubYG2PaymentsService: покупка не завершена: '{purchaseId}'.",
            this);
    }

    private void FailPurchaseRequest(string reason)
    {
        _purchaseRequestPending = false;
        Debug.LogError($"CyberClubYG2PaymentsService: {reason}", this);
        _feedbackPresenter?.Show(PurchaseFailureReason.TransactionFailed);
        OnPurchaseFailed?.Invoke(_premiumZoneProductId);
    }

    private IEnumerator CatalogTimeoutRoutine()
    {
        yield return new WaitForSecondsRealtime(
            Mathf.Clamp(_catalogTimeoutSeconds, 10f, 15f));

        _catalogTimeoutRoutine = null;
        _catalogRequestStarted = false;

        if (ProductLoadState != PremiumProductLoadState.Loading)
            yield break;

        SetLoadFailure(
            PremiumProductLoadState.Failed,
            $"Timeout запроса каталога после {_catalogTimeoutSeconds:0.#} с. " +
            $"Product ID: '{_premiumZoneProductId}'.");
    }

    private void StopCatalogTimeout()
    {
        if (_catalogTimeoutRoutine == null)
            return;

        StopCoroutine(_catalogTimeoutRoutine);
        _catalogTimeoutRoutine = null;
    }

    private void SetLoadFailure(PremiumProductLoadState state, string error)
    {
        StopCatalogTimeout();
        _catalogRequestStarted = false;
        LastCatalogError = error;
        Debug.LogError($"CyberClubYG2PaymentsService: {error}", this);
        SetLoadState(state);
    }

    private void SetLoadState(PremiumProductLoadState state)
    {
        if (ProductLoadState == state)
            return;

        ProductLoadState = state;
        OnProductLoadStateChanged?.Invoke(state);
    }

    private string GetCatalogIdsForLog()
    {
        if (YG2.purchases == null || YG2.purchases.Length == 0)
            return "<empty>";

        string result = string.Empty;

        for (int i = 0; i < YG2.purchases.Length; i++)
        {
            if (i > 0)
                result += ", ";

            result += YG2.purchases[i]?.id ?? "<null>";
        }

        return result;
    }

    private void ValidateReferences()
    {
        if (_saveLoadManager == null)
            ReportMissing(nameof(_saveLoadManager));
        if (_premiumLocationUnlocker == null)
            ReportMissing(nameof(_premiumLocationUnlocker));
        if (_feedbackPresenter == null)
            ReportMissing(nameof(_feedbackPresenter));
    }

    private void ReportMissing(string fieldName)
    {
        Debug.LogError(
            $"CyberClubYG2PaymentsService: поле {fieldName} не назначено на GameObject '{name}'.",
            this);
    }
}
