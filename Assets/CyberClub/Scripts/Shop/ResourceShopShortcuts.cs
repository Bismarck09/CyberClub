using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceShopShortcuts : MonoBehaviour
{
    [SerializeField] private RectTransform _coinsWallet;
    [SerializeField] private RectTransform _gemsWallet;
    [SerializeField] private GameObject _shopRoot;
    [SerializeField] private UniversalShopWindow _shopWindow;

    private Button _coinsButton;
    private Button _gemsButton;

    private void Awake()
    {
        _coinsButton = CreateShortcut(_coinsWallet, "CoinsShopShortcut");
        _gemsButton = CreateShortcut(_gemsWallet, "GemsShopShortcut");

        if (_coinsButton != null)
            _coinsButton.onClick.AddListener(OpenCoins);

        if (_gemsButton != null)
            _gemsButton.onClick.AddListener(OpenGems);
    }

    private void OnDestroy()
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
            Debug.LogError("ResourceShopShortcuts: не назначен существующий магазин.", this);
            return;
        }

        if (!_shopRoot.activeSelf)
            _shopRoot.SetActive(true);

        _shopWindow.ShowResourcesAndSelect(actionType);
    }

    private Button CreateShortcut(RectTransform wallet, string objectName)
    {
        if (wallet == null)
            return null;

        Transform existing = wallet.Find(objectName);

        if (existing != null)
            return existing.GetComponent<Button>();

        GameObject gameObject = new GameObject(objectName, typeof(RectTransform));
        gameObject.layer = wallet.gameObject.layer;
        gameObject.transform.SetParent(wallet, false);

        RectTransform rect = (RectTransform)gameObject.transform;
        rect.anchorMin = new Vector2(1f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.anchoredPosition = new Vector2(-12f, 0f);
        rect.sizeDelta = new Vector2(58f, 58f);

        Image image = gameObject.AddComponent<Image>();
        image.color = new Color(0.05f, 0.62f, 0.86f, 0.96f);

        Button button = gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        GameObject labelObject = new GameObject("Plus", typeof(RectTransform));
        labelObject.layer = gameObject.layer;
        labelObject.transform.SetParent(gameObject.transform, false);

        RectTransform labelRect = (RectTransform)labelObject.transform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.text = "+";
        label.fontSize = 42f;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;

        return button;
    }
}
