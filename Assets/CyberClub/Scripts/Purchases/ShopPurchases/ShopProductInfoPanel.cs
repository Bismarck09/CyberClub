using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopProductInfoPanel : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _descriptionText;

    [Header("Info rows")]
    [SerializeField] private TMP_Text _durationText;
    [SerializeField] private TMP_Text _priceText;

    [Header("Buttons")]
    [SerializeField] private Button _buyButton;

    private ShopProductConfig _currentConfig;
    private PotionPurchaseService _purchaseService;

    public ShopProductConfig CurrentConfig => _currentConfig;

    public void Construct(PotionPurchaseService purchaseService)
    {
        _purchaseService = purchaseService;
    }

    private void OnEnable()
    {
        if (_buyButton != null)
            _buyButton.onClick.AddListener(BuyCurrentProduct);
    }

    private void OnDisable()
    {
        if (_buyButton != null)
            _buyButton.onClick.RemoveListener(BuyCurrentProduct);
    }

    public void Show(ShopProductConfig config)
    {
        _currentConfig = config;

        if (config == null)
        {
            Clear();
            return;
        }

        if (_icon != null)
        {
            _icon.enabled = config.Icon != null;
            _icon.sprite = config.Icon;
        }

        //if (_titleText != null)
            //_titleText.text = config.DisplayName;

        if (_descriptionText != null)
            _descriptionText.text = config.Description;

        if (_durationText != null)
            _durationText.text = config.DurationText;

        if (_priceText != null)
            _priceText.text = config.PriceGems.ToString();

        if (_buyButton != null)
            _buyButton.interactable = config.Category == ShopProductCategory.Potions;
    }

    private void Clear()
    {
        if (_icon != null)
        {
            _icon.sprite = null;
            _icon.enabled = false;
        }

        //if (_titleText != null)
            //_titleText.text = string.Empty;

        if (_descriptionText != null)
            _descriptionText.text = string.Empty;

        if (_durationText != null)
            _durationText.text = string.Empty;

        if (_priceText != null)
            _priceText.text = string.Empty;

        if (_buyButton != null)
            _buyButton.interactable = false;
    }

    private void BuyCurrentProduct()
    {
        if (_currentConfig == null || _purchaseService == null)
            return;

        _purchaseService.TryBuy(_currentConfig);
    }
}
