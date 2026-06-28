using UnityEngine;

public class ShopPageController : MonoBehaviour
{
    [SerializeField] private ShopActionService _actionService;
    [SerializeField] private ShopProductInfoPanel _infoPanel;
    [SerializeField] private ShopProductButton[] _productButtons;
    [SerializeField] private bool _selectFirstProductOnEnable = true;

    private ShopProductButton _selectedButton;

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        Initialize();

        if (_selectFirstProductOnEnable)
            SelectFirstAvailableProduct();
    }

    private void Initialize()
    {
        if (_infoPanel != null)
            _infoPanel.Initialize(_actionService);

        if (_productButtons == null)
            return;

        foreach (ShopProductButton button in _productButtons)
        {
            if (button == null)
                continue;

            button.Initialize(this);
            button.SetSelected(false);
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

    private void SelectFirstAvailableProduct()
    {
        if (_productButtons == null || _productButtons.Length == 0)
            return;

        foreach (ShopProductButton button in _productButtons)
        {
            if (button == null || button.gameObject.activeInHierarchy == false || button.Product == null)
                continue;

            SelectProduct(button);
            return;
        }

        if (_infoPanel != null)
            _infoPanel.Show(null);
    }
}