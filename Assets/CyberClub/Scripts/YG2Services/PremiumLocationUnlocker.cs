using System;
using UnityEngine;

public class PremiumLocationUnlocker : MonoBehaviour
{
    [SerializeField] private GameObject _premiumBarrier;
    [SerializeField] private ZonePurchaseConfig _premiumZoneConfig;
    [SerializeField] private GameObject _premiumLockUI;
    [SerializeField] private GameObject _premiumBuyButton;

    private bool _isUnlocked;

    public bool IsUnlocked => _isUnlocked;
    public event Action OnPremiumUnlocked;

    public bool UnlockPremiumLocation()
    {
        if (_isUnlocked)
            return false;

        _isUnlocked = true;
        _premiumZoneConfig?.CommitUnlockedState();
        ApplyVisualState();
        InvokeUnlockedSafely();
        return true;
    }

    public void RestoreUnlockedState(bool isUnlocked)
    {
        _isUnlocked = isUnlocked;
        _premiumZoneConfig?.RestoreUnlockedState(isUnlocked);
        ApplyVisualState();
    }

    private void ApplyVisualState()
    {
        if (_premiumBarrier != null)
            _premiumBarrier.SetActive(!_isUnlocked);
        if (_premiumLockUI != null)
            _premiumLockUI.SetActive(!_isUnlocked);
        if (_premiumBuyButton != null)
            _premiumBuyButton.SetActive(!_isUnlocked);
    }

    private void InvokeUnlockedSafely()
    {
        if (OnPremiumUnlocked == null)
            return;

        foreach (Delegate handler in OnPremiumUnlocked.GetInvocationList())
        {
            try
            {
                ((Action)handler).Invoke();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }
    }
}
