using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ShopProductButton : MonoBehaviour
{
    [SerializeField] private ShopProductConfig _config;

    [Header("UI")]
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _durationText;

    [Header("Selection visuals")]
    [SerializeField] private GameObject _normalFrame;
    [SerializeField] private GameObject _selectedFrame;

    private Button _button;
    private UniversalShopWindow _shopWindow;

    public ShopProductConfig Config => _config;

    public void Construct(UniversalShopWindow shopWindow)
    {
        _shopWindow = shopWindow;
        Refresh();
    }

    private void Awake()
    {
        _button = GetComponent<Button>();
        Refresh();
    }

    private void OnEnable()
    {
        if (_button != null)
            _button.onClick.AddListener(Select);
    }

    private void OnDisable()
    {
        if (_button != null)
            _button.onClick.RemoveListener(Select);
    }

    public void SetConfig(ShopProductConfig config)
    {
        _config = config;
        Refresh();
    }

    public void SetSelected(bool value)
    {
        //if (_normalFrame != null)
        //    _normalFrame.SetActive(!value);

        //if (_selectedFrame != null)
        //    _selectedFrame.SetActive(value);
    }

    private void Refresh()
    {
        if (_config == null)
            return;

        if (_icon != null)
        {
            _icon.enabled = _config.Icon != null;
            _icon.sprite = _config.Icon;
        }

        if (_titleText != null)
            _titleText.text = _config.DisplayName;

        //if (_durationText != null)
            //_durationText.text = _config.DurationText;
    }

    private void Select()
    {
        if (_shopWindow == null)
            return;

        _shopWindow.SelectProduct(this);
    }
}