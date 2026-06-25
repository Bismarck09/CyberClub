using UnityEngine;
using System.Collections;

public class Purchase : MonoBehaviour
{
    [SerializeField] private float _purchaseCooldown = 0.5f;

    private bool _canPurchase = true;

    public void Buy(GameObject purchasableObject)
    {
        if (!_canPurchase)
            return;

        if (purchasableObject.TryGetComponent(out IPurchasable purchasable))
        {
            purchasable.Buy();
            StartCoroutine(PurchaseCooldown());
        }
    }

    private IEnumerator PurchaseCooldown()
    {
        _canPurchase = false;
        yield return new WaitForSeconds(_purchaseCooldown);
        _canPurchase = true;
    }

}
