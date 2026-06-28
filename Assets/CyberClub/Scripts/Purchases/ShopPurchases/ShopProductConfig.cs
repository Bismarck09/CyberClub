using UnityEngine;

[CreateAssetMenu(menuName = "CyberClub/Shop/Product Config")]
public class ShopProductConfig : ScriptableObject
{
    [Header("Action")]
    [SerializeField] private ShopProductActionType _actionType;

    [Header("Potion settings")]
    [SerializeField] private PotionType _potionType;
    [SerializeField] private float _durationSeconds;
    [SerializeField] private int _effectMultiplier = 1;

    [Header("Resource reward settings")]
    [SerializeField] private int _rewardAmount;

    [Header("Price")]
    [SerializeField] private int _priceGems;

    [Header("UI")]
    [SerializeField] private Sprite _icon;
    [TextArea(2, 5)]
    [SerializeField] private string _description;
    [SerializeField] private string _durationText;
    [SerializeField] private string _buttonText = "Купить";

    public ShopProductActionType ActionType => _actionType;
    public PotionType PotionType => _potionType;
    public float DurationSeconds => _durationSeconds;
    public int EffectMultiplier => Mathf.Max(1, _effectMultiplier);
    public int RewardAmount => Mathf.Max(0, _rewardAmount);
    public int PriceGems => Mathf.Max(0, _priceGems);
    public Sprite Icon => _icon;
    public string Description => _description;
    public string DurationText => _durationText;
    public string ButtonText => _buttonText;

    public bool HasDuration => string.IsNullOrWhiteSpace(_durationText) == false;
    public bool HasPrice => _priceGems > 0;
}