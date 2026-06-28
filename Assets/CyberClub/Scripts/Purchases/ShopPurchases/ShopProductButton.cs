using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ShopProductButton : MonoBehaviour
{
    [SerializeField] private ShopProductConfig _product;

    [Header("Selection frames")]
    [SerializeField] private GameObject _defaultFrame;
    [SerializeField] private GameObject _selectedFrame;

    private Button _button;
    private ShopPageController _pageController;
    private UniversalShopWindow _universalShopWindow;

    public ShopProductConfig Product => _product;

    // Совместимость со старым UniversalShopWindow.
    public ShopProductConfig Config => _product;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (_button == null)
            _button = GetComponent<Button>();

        _button.onClick.AddListener(Select);
    }

    private void OnDisable()
    {
        if (_button != null)
            _button.onClick.RemoveListener(Select);
    }

    public void Initialize(ShopPageController pageController)
    {
        _pageController = pageController;
        _universalShopWindow = null;

        if (_button == null)
            _button = GetComponent<Button>();

        SetSelected(false);
    }

    // Совместимость со старым UniversalShopWindow.
    public void Construct(UniversalShopWindow window)
    {
        _universalShopWindow = window;
        _pageController = null;

        if (_button == null)
            _button = GetComponent<Button>();

        SetSelected(false);
    }

    public void SetProduct(ShopProductConfig product)
    {
        _product = product;
    }

    // Совместимость со старым кодом.
    public void SetConfig(ShopProductConfig product)
    {
        SetProduct(product);
    }

    public void SetSelected(bool value)
    {
        if (_defaultFrame != null)
            _defaultFrame.SetActive(!value);

        if (_selectedFrame != null)
            _selectedFrame.SetActive(value);
    }

    private void Select()
    {
        if (_product == null)
        {
            Debug.LogWarning($"{name}: на кнопке товара не назначен ShopProductConfig.");
            return;
        }

        if (_pageController != null)
        {
            _pageController.SelectProduct(this);
            return;
        }

        if (_universalShopWindow != null)
        {
            _universalShopWindow.SelectProduct(this);
            return;
        }

        Debug.LogWarning($"{name}: кнопка товара не привязана ни к ShopPageController, ни к UniversalShopWindow.");
    }
}
