using UnityEngine;

public class DevicePurchaseSound : MonoBehaviour
{
    [SerializeField] private AudioSource _purchaseSoundSource;
    [SerializeField] private DevicePurchase _devicePurchase;

    private void OnEnable()
    {
        _devicePurchase.OnDevicePurchased += PlaySound;
    }

    private void OnDisable()
    {
        _devicePurchase.OnDevicePurchased -= PlaySound;
    }

    private void PlaySound()
    {
        _purchaseSoundSource.Play();
    }
}
