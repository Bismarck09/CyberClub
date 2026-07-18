using System;
using UnityEngine;

public class ZonePurchase : MonoBehaviour
{
    [SerializeField] private CoinsData _coinsData;
    [SerializeField] private SaveLoadManager _saveLoadManager;

    // Событие можно оставить для квестов, звука и аналитики.
    // BarrierDissolve больше на него не подписывается.
    public event Action OnZonePurchased;

    // ИЗМЕНЕНО: метод получает конкретную покупаемую зону.
    public void Buy(ZonePurchaseConfig config)
    {
        if (!CanBuy(config))
            return;

        BarrierDissolve barrier =
            config.GetComponent<BarrierDissolve>();

        int price = config.ZonePrice;

        // ИЗМЕНЕНО: списание происходит ровно один раз
        // и только после полной проверки объекта.
        if (!_coinsData.TryBuy(price))
            return;

        bool unlockStarted = barrier.TryUnlock(() =>
        {
            // Барьер к этому моменту уже выключен,
            // поэтому ZonesSaveModule увидит зону открытой.
            _saveLoadManager?.SaveGame();
        });

        if (!unlockStarted)
        {
            // Страховка: если после оплаты открытие не запустилось,
            // полностью возвращаем деньги.
            _coinsData.AddResource(price, 1f);

            Debug.LogError(
                $"ZonePurchase: не удалось открыть {config.name}. " +
                $"Деньги возвращены.");

            return;
        }

        OnZonePurchased?.Invoke();
    }

    // ИЗМЕНЕНО: чистая проверка без изменения CoinsData.
    public bool CanBuy(ZonePurchaseConfig config)
    {
        if (config == null || _coinsData == null)
            return false;

        if (config.BarrierObject == null ||
            !config.BarrierObject.activeSelf)
        {
            return false;
        }

        BarrierDissolve barrier =
            config.GetComponent<BarrierDissolve>();

        if (barrier == null || !barrier.CanUnlock)
            return false;

        return _coinsData.CurrentCoins >= config.ZonePrice;
    }
}
