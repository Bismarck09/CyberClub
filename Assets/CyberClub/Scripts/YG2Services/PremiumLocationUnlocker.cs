using System;
using UnityEngine;

public class PremiumLocationUnlocker : MonoBehaviour
{
    [SerializeField] private GameObject _premiumBarrier;
    [SerializeField] private GameObject _premiumLockUI;
    [SerializeField] private GameObject _premiumBuyButton;

    private bool _isUnlocked;
    public bool IsUnlocked => _isUnlocked;
    public event Action OnPremiumUnlocked;

    public void UnlockPremiumLocation()
    {
        if (_isUnlocked)
            return;

        _isUnlocked = true;
        ApplyVisualState();
        OnPremiumUnlocked?.Invoke();
    }

    public void RestoreUnlockedState(bool isUnlocked)
    {
        _isUnlocked = isUnlocked;
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
}