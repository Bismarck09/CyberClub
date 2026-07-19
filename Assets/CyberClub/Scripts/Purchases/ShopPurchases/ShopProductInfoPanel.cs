using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopProductInfoPanel : MonoBehaviour
{
    [Header("Required UI")]
    [SerializeField] private Image _productIcon;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private Button _actionButton;

    [Header("Optional UI for potions")]
    [SerializeField] private TMP_Text _durationText;
    [SerializeField] private TMP_Text _priceText;

    [Header("Optional roots")]
    [SerializeField] private GameObject _durationRoot;
    [SerializeField] private GameObject _priceRoot;

    [Header("Optional button label")]
    [SerializeField] private TMP_Text _actionButtonText;

    private ShopProductConfig _currentProduct;
    private ShopActionService _actionService;

    public void Initialize(ShopActionService actionService)
    {
        _actionService = actionService;
    }

    private void OnEnable()
    {
        if (_actionButton != null)
            _actionButton.onClick.AddListener(ExecuteCurrentProduct);
    }

    private void OnDisable()
    {
        if (_actionButton != null)
            _actionButton.onClick.RemoveListener(ExecuteCurrentProduct);
    }

    public void Show(ShopProductConfig product)
    {
        _currentProduct = product;

        if (product == null)
        {
            Clear();
            return;
        }

        if (_productIcon != null)
        {
            _productIcon.enabled = product.Icon != null;
            _productIcon.sprite = product.Icon;
        }

        if (_descriptionText != null)
            _descriptionText.text = product.Description;

        bool showDuration = product.HasDuration && _durationText != null;

        if (_durationRoot != null)
            _durationRoot.SetActive(showDuration);

        if (_durationText != null)
            _durationText.text = showDuration ? product.DurationText : string.Empty;

        bool showPrice = product.HasPrice && _priceText != null;

        if (_priceRoot != null)
            _priceRoot.SetActive(showPrice);

        if (_priceText != null)
            _priceText.text = showPrice ? ResourceValueFormatter.Format(product.PriceGems) : string.Empty;

        if (_actionButtonText != null)
            _actionButtonText.text = string.IsNullOrWhiteSpace(product.ButtonText) ? "Купить" : product.ButtonText;

        if (_actionButton != null)
            _actionButton.interactable = true;
    }

    private void ExecuteCurrentProduct()
    {
        if (_currentProduct == null)
        {
            Debug.LogWarning("ShopProductInfoPanel: нет выбранного товара.");
            return;
        }

        if (_actionService == null)
        {
            Debug.LogWarning("ShopProductInfoPanel: не назначен ShopActionService.");
            return;
        }

        _actionService.Execute(_currentProduct);
    }

    private void Clear()
    {
        if (_productIcon != null)
        {
            _productIcon.sprite = null;
            _productIcon.enabled = false;
        }

        if (_descriptionText != null)
            _descriptionText.text = string.Empty;

        if (_durationText != null)
            _durationText.text = string.Empty;

        if (_priceText != null)
            _priceText.text = string.Empty;

        if (_durationRoot != null)
            _durationRoot.SetActive(false);

        if (_priceRoot != null)
            _priceRoot.SetActive(false);

        if (_actionButton != null)
            _actionButton.interactable = false;
    }
}
