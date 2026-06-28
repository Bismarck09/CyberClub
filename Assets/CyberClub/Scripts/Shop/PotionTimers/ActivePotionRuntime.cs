using UnityEngine;

public class ActivePotionRuntime
{
    public ShopProductConfig Product { get; private set; }
    public PotionType PotionType { get; private set; }
    public float Duration { get; private set; }
    public float RemainingTime { get; private set; }

    public float Progress01 => Duration <= 0f ? 0f : Mathf.Clamp01(RemainingTime / Duration);

    public ActivePotionRuntime(ShopProductConfig product)
    {
        Restart(product);
    }

    public void Restart(ShopProductConfig product)
    {
        Product = product;
        PotionType = product.PotionType;
        Duration = Mathf.Max(0.1f, product.DurationSeconds);
        RemainingTime = Duration;
    }

    public void Tick(float deltaTime)
    {
        RemainingTime = Mathf.Max(0f, RemainingTime - deltaTime);
    }
}
