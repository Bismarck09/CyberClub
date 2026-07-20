using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ZonePurchasePopupView : MonoBehaviour
{
    [Header("Authored hierarchy")]
    [SerializeField] private GameObject _root;
    [SerializeField] private RectTransform _safeAreaRoot;

    [Header("Content")]
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _gameplayBenefitText;
    [SerializeField] private TMP_Text _progressionHintText;
    [SerializeField] private TMP_Text _capacityText;
    [SerializeField] private TMP_Text _priceText;
    [SerializeField] private TMP_Text _statusText;
    [SerializeField] private Image _previewImage;
    [SerializeField] private Image _currencyIcon;

    [Header("Actions")]
    [SerializeField] private Button _buyButton;
    [SerializeField] private TMP_Text _buyButtonText;
    [SerializeField] private Button _cancelButton;

    private Rect _lastSafeArea;

    public event Action BuyRequested;
    public event Action CancelRequested;

    private void Awake()
    {
        ValidateReferences();
    }

    private void OnEnable()
    {
        if (_buyButton != null)
            _buyButton.onClick.AddListener(NotifyBuyRequested);

        if (_cancelButton != null)
            _cancelButton.onClick.AddListener(NotifyCancelRequested);

        ApplySafeArea();
    }

    private void OnDisable()
    {
        if (_buyButton != null)
            _buyButton.onClick.RemoveListener(NotifyBuyRequested);

        if (_cancelButton != null)
            _cancelButton.onClick.RemoveListener(NotifyCancelRequested);
    }

    private void Update()
    {
        if (_lastSafeArea != Screen.safeArea)
            ApplySafeArea();
    }

    public void SetContent(
        string title,
        string description,
        string gameplayBenefit,
        string progressionHint,
        string capacity,
        string price,
        Sprite previewSprite)
    {
        SetText(_titleText, title);
        SetText(_descriptionText, description);
        SetText(_gameplayBenefitText, gameplayBenefit);
        SetText(_progressionHintText, progressionHint);
        SetText(_capacityText, capacity);
        SetText(_priceText, price);

        if (_previewImage != null)
        {
            _previewImage.sprite = previewSprite;
            _previewImage.gameObject.SetActive(previewSprite != null);
        }
    }

    public void SetCurrencyIcon(Sprite sprite)
    {
        if (_currencyIcon == null)
            return;

        _currencyIcon.sprite = sprite;
        _currencyIcon.gameObject.SetActive(sprite != null);
    }

    public void SetStatus(string message, bool isError)
    {
        if (_statusText == null)
            return;

        _statusText.text = message;
        _statusText.color = isError
            ? new Color(1f, 0.43f, 0.4f)
            : new Color(0.78f, 0.86f, 1f);
    }

    public void SetControls(bool buyInteractable, bool cancelInteractable, string buyButtonText)
    {
        if (_buyButton != null)
            _buyButton.interactable = buyInteractable;

        if (_cancelButton != null)
            _cancelButton.interactable = cancelInteractable;

        SetText(_buyButtonText, buyButtonText);
    }

    public void Show()
    {
        if (_root == null)
        {
            ReportMissing(nameof(_root));
            return;
        }

        _root.SetActive(true);
        //ApplySafeArea();
    }

    public void Hide()
    {
        if (_root != null)
            _root.SetActive(false);
    }

    private void NotifyBuyRequested()
    {
        BuyRequested?.Invoke();
    }

    private void NotifyCancelRequested()
    {
        CancelRequested?.Invoke();
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

    private static void SetText(TMP_Text target, string value)
    {
        if (target != null)
            target.text = value ?? string.Empty;
    }

    private void ValidateReferences()
    {
        if (_root == null)
            ReportMissing(nameof(_root));
        if (_safeAreaRoot == null)
            ReportMissing(nameof(_safeAreaRoot));
        if (_titleText == null)
            ReportMissing(nameof(_titleText));
        if (_descriptionText == null)
            ReportMissing(nameof(_descriptionText));
        if (_gameplayBenefitText == null)
            ReportMissing(nameof(_gameplayBenefitText));
        if (_progressionHintText == null)
            ReportMissing(nameof(_progressionHintText));
        if (_capacityText == null)
            ReportMissing(nameof(_capacityText));
        if (_priceText == null)
            ReportMissing(nameof(_priceText));
        if (_statusText == null)
            ReportMissing(nameof(_statusText));
        if (_buyButton == null)
            ReportMissing(nameof(_buyButton));
        if (_buyButtonText == null)
            ReportMissing(nameof(_buyButtonText));
        if (_cancelButton == null)
            ReportMissing(nameof(_cancelButton));
    }

    private void ReportMissing(string fieldName)
    {
        Debug.LogError($"ZonePurchasePopupView: поле {fieldName} не назначено на GameObject '{name}'.", this);
    }
}
