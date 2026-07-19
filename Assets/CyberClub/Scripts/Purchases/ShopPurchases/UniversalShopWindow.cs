using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UniversalShopWindow : MonoBehaviour
{
    [Header("New service")]
    [SerializeField] private ShopActionService _actionService;

    [Header("Products")]
    [SerializeField] private List<ShopProductConfig> _products = new();

    [Header("UI")]
    [SerializeField] private ShopProductInfoPanel _infoPanel;
    [SerializeField] private List<ShopProductButton> _productButtons = new();

    [Header("Tabs")]
    [SerializeField] private Button _potionsTabButton;
    [SerializeField] private Button _resourcesTabButton;

    private readonly List<ShopProductButton> _activeButtons = new();
    private ShopProductButton _selectedButton;
    private bool _showPotions = true;

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        if (_potionsTabButton != null)
            _potionsTabButton.onClick.AddListener(ShowPotions);

        if (_resourcesTabButton != null)
            _resourcesTabButton.onClick.AddListener(ShowResources);

        Initialize();
        RebuildCurrentCategory();
    }

    private void OnDisable()
    {
        if (_potionsTabButton != null)
            _potionsTabButton.onClick.RemoveListener(ShowPotions);

        if (_resourcesTabButton != null)
            _resourcesTabButton.onClick.RemoveListener(ShowResources);
    }

    private void Initialize()
    {
        if (_infoPanel != null)
            _infoPanel.Initialize(_actionService);

        foreach (ShopProductButton button in _productButtons)
        {
            if (button == null)
                continue;

            button.Construct(this);
            button.SetSelected(false);
        }
    }

    public void ShowPotions()
    {
        _showPotions = true;
        RebuildCurrentCategory();
    }

    public void ShowResources()
    {
        _showPotions = false;
        RebuildCurrentCategory();
    }

    public void ShowResourcesAndSelect(ShopProductActionType actionType)
    {
        _showPotions = false;
        RebuildCurrentCategory();

        foreach (ShopProductButton button in _activeButtons)
        {
            if (button != null && button.Product != null && button.Product.ActionType == actionType)
            {
                SelectProduct(button);
                return;
            }
        }
    }

    public void SelectProduct(ShopProductButton button)
    {
        if (button == null || button.Product == null)
            return;

        if (_selectedButton != null)
            _selectedButton.SetSelected(false);

        _selectedButton = button;
        _selectedButton.SetSelected(true);

        if (_infoPanel != null)
            _infoPanel.Show(button.Product);
    }

    private void RebuildCurrentCategory()
    {
        _activeButtons.Clear();

        List<ShopProductConfig> products = GetCurrentProducts();

        for (int i = 0; i < _productButtons.Count; i++)
        {
            ShopProductButton button = _productButtons[i];

            if (button == null)
                continue;

            bool hasProduct = i < products.Count;
            button.gameObject.SetActive(hasProduct);
            button.SetSelected(false);

            if (!hasProduct)
                continue;

            button.SetConfig(products[i]);
            _activeButtons.Add(button);
        }

        _selectedButton = null;

        if (_activeButtons.Count > 0)
            SelectProduct(_activeButtons[0]);
        else if (_infoPanel != null)
            _infoPanel.Show(null);
    }

    private List<ShopProductConfig> GetCurrentProducts()
    {
        if (_showPotions)
            return _products.FindAll(product => product != null && product.ActionType == ShopProductActionType.Potion);

        return _products.FindAll(product =>
            product != null &&
            (product.ActionType == ShopProductActionType.RewardGems ||
             product.ActionType == ShopProductActionType.RewardCoins));
    }
}
