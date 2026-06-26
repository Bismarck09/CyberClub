using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UniversalShopWindow : MonoBehaviour
{
    [Header("Services")]
    [SerializeField] private PotionPurchaseService _purchaseService;

    [Header("Products")]
    [SerializeField] private List<ShopProductConfig> _products = new();

    [Header("UI")]
    [SerializeField] private ShopProductInfoPanel _infoPanel;
    [SerializeField] private List<ShopProductButton> _productButtons = new();

    [Header("Tabs")]
    [SerializeField] private Button _potionsTabButton;
    [SerializeField] private Button _gemsTabButton;

    private readonly List<ShopProductButton> _activeButtons = new();
    private ShopProductButton _selectedButton;
    private ShopProductCategory _currentCategory = ShopProductCategory.Potions;

    private void Awake()
    {
        if (_infoPanel != null)
            _infoPanel.Construct(_purchaseService);

        foreach (ShopProductButton button in _productButtons)
        {
            if (button == null)
                continue;

            button.Construct(this);
            button.SetSelected(false);
        }
    }

    private void OnEnable()
    {
        if (_potionsTabButton != null)
            _potionsTabButton.onClick.AddListener(ShowPotions);

        if (_gemsTabButton != null)
            _gemsTabButton.onClick.AddListener(ShowGems);

        RebuildCurrentCategory();
    }

    private void OnDisable()
    {
        if (_potionsTabButton != null)
            _potionsTabButton.onClick.RemoveListener(ShowPotions);

        if (_gemsTabButton != null)
            _gemsTabButton.onClick.RemoveListener(ShowGems);
    }

    public void ShowPotions()
    {
        ShowCategory(ShopProductCategory.Potions);
    }

    public void ShowGems()
    {
        ShowCategory(ShopProductCategory.Gems);
    }

    public void ShowCategory(ShopProductCategory category)
    {
        _currentCategory = category;
        RebuildCurrentCategory();
    }

    public void SelectProduct(ShopProductButton button)
    {
        if (button == null || button.Config == null)
            return;

        if (_selectedButton != null)
            _selectedButton.SetSelected(false);

        _selectedButton = button;
        _selectedButton.SetSelected(true);

        if (_infoPanel != null)
            _infoPanel.Show(button.Config);
    }

    private void RebuildCurrentCategory()
    {
        _activeButtons.Clear();

        List<ShopProductConfig> categoryProducts = _products.FindAll(product => product != null && product.Category == _currentCategory);

        for (int i = 0; i < _productButtons.Count; i++)
        {
            ShopProductButton button = _productButtons[i];

            if (button == null)
                continue;

            bool hasProduct = i < categoryProducts.Count;
            button.gameObject.SetActive(hasProduct);
            button.SetSelected(false);

            if (!hasProduct)
                continue;

            button.SetConfig(categoryProducts[i]);
            button.Construct(this);
            _activeButtons.Add(button);
        }

        _selectedButton = null;

        if (_activeButtons.Count > 0)
            SelectProduct(_activeButtons[0]);
        else if (_infoPanel != null)
            _infoPanel.Show(null);
    }
}
