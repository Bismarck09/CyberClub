using UnityEngine;
using UnityEngine.UI;

public class ResourceShopShortcuts : MonoBehaviour
{
    [Header("Authored shortcut buttons")]
    [SerializeField] private Button _coinsButton;
    [SerializeField] private Button _gemsButton;
    [SerializeField] private GameObject _shopRoot;
    [SerializeField] private UniversalShopWindow _shopWindow;

    private void OnEnable()
    {
        ValidateReferences();

        if (_coinsButton != null)
            _coinsButton.onClick.AddListener(OpenCoins);

        if (_gemsButton != null)
            _gemsButton.onClick.AddListener(OpenGems);
    }

    private void OnDisable()
    {
        if (_coinsButton != null)
            _coinsButton.onClick.RemoveListener(OpenCoins);

        if (_gemsButton != null)
            _gemsButton.onClick.RemoveListener(OpenGems);
    }

    public void OpenCoins()
    {
        Open(ShopProductActionType.RewardCoins);
    }

    public void OpenGems()
    {
        Open(ShopProductActionType.RewardGems);
    }

    private void Open(ShopProductActionType actionType)
    {
        if (_shopRoot == null || _shopWindow == null)
        {
            Debug.LogError($"ResourceShopShortcuts: магазин не настроен на GameObject '{name}'.", this);
            return;
        }

        if (!_shopRoot.activeSelf)
            _shopRoot.SetActive(true);

        _shopWindow.ShowResourcesAndSelect(actionType);
    }

    private void ValidateReferences()
    {
        if (_coinsButton == null)
            ReportMissing(nameof(_coinsButton));
        if (_gemsButton == null)
            ReportMissing(nameof(_gemsButton));
        if (_shopRoot == null)
            ReportMissing(nameof(_shopRoot));
        if (_shopWindow == null)
            ReportMissing(nameof(_shopWindow));
    }

    private void ReportMissing(string fieldName)
    {
        Debug.LogError($"ResourceShopShortcuts: поле {fieldName} не назначено на GameObject '{name}'.", this);
    }
}
