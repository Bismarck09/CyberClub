using UnityEngine;

[CreateAssetMenu(menuName = "Game/Zone Device Config")]
public class ZoneDeviceConfig : ScriptableObject
{
    [Header("Device")]
    [SerializeField] private DeviceType _deviceType;
    [SerializeField] private GameObject _devicePrefab;

    [Header("Income")]
    [SerializeField] private int _priceOfHourCoins;

    [Header("Gems Drop")]
    [Range(0, 100)]
    [SerializeField] private int _gemsDropChancePercent;
    [SerializeField] private int _minGemsReward;
    [SerializeField] private int _maxGemsReward;

    [Header("Purchase Config")]
    [SerializeField] private int _baseDevicePrice;
    [Range(0f, 500f)]
    [SerializeField] private float _devicePriceGrowthPercent = 10f;

    public DeviceType DeviceType => _deviceType;
    public GameObject DevicePrefab => _devicePrefab;

    public int PriceOfHourCoins => Mathf.Max(0, _priceOfHourCoins);

    // Оставлено для обратной совместимости: старый код может всё ещё читать PriceOfHourGems.
    // Новый доход гемов надо брать через RollGemsReward().
    public int PriceOfHourGems => 0;

    public int GemsDropChancePercent => Mathf.Clamp(_gemsDropChancePercent, 0, 100);
    public int MinGemsReward => Mathf.Max(0, _minGemsReward);
    public int MaxGemsReward => Mathf.Max(MinGemsReward, _maxGemsReward);

    public int BaseDevicePrice => Mathf.Max(0, _baseDevicePrice);
    public float DevicePriceGrowthPercent => Mathf.Max(0f, _devicePriceGrowthPercent);

    // Оставлено для старых UI-скриптов. Это базовая цена, не runtime-цена.
    public int DevicePrice => BaseDevicePrice;

    public int CalculateDevicePrice(int purchasedDeviceCount)
    {
        int price = BaseDevicePrice;
        int safePurchasedCount = Mathf.Max(0, purchasedDeviceCount);
        float growth = DevicePriceGrowthPercent / 100f;

        for (int i = 0; i < safePurchasedCount; i++)
            price += Mathf.RoundToInt(price * growth);

        return Mathf.Max(0, price);
    }

    public int RollGemsReward()
    {
        if (GemsDropChancePercent <= 0)
            return 0;

        if (MaxGemsReward <= 0)
            return 0;

        bool isDropped = Random.Range(0, 100) < GemsDropChancePercent;

        if (!isDropped)
            return 0;

        return Random.Range(MinGemsReward, MaxGemsReward + 1);
    }
}
