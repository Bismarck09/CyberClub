using UnityEngine;

[CreateAssetMenu(menuName = "CyberClub/Shop/Product Config")]
public class ShopProductConfig : ScriptableObject
{
    [Header("Category")]
    [SerializeField] private ShopProductCategory _category = ShopProductCategory.Potions;

    [Header("Potion")]
    [SerializeField] private PotionType _potionType;

    [Header("UI")]
    [SerializeField] private string _displayName;
    [TextArea(2, 4)]
    [SerializeField] private string _description;
    [SerializeField] private string _durationText;
    [SerializeField] private Sprite _icon;

    [Header("Price")]
    [SerializeField] private int _priceGems;

    [Header("Effect")]
    [SerializeField] private float _durationSeconds = 300f;
    [SerializeField] private float _multiplier = 2f;

    public ShopProductCategory Category => _category;
    public PotionType PotionType => _potionType;
    public string DisplayName => _displayName;
    public string Description => _description;
    public string DurationText => _durationText;
    public Sprite Icon => _icon;
    public int PriceGems => _priceGems;
    public float DurationSeconds => _durationSeconds;
    public float Multiplier => _multiplier;
}