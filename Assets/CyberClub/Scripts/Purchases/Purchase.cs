using UnityEngine;

public class Purchase : MonoBehaviour
{
    public void Buy(GameObject purchasableObject)
    {
        if (purchasableObject.TryGetComponent(out IPurchasable purchasable))
            purchasable.Buy();
    }

}
