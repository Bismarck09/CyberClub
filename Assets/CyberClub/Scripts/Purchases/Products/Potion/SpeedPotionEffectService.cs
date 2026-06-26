using System;
using System.Collections;
using UnityEngine;

public class SpeedPotionEffectService : MonoBehaviour
{
    public static SpeedPotionEffectService Current { get; private set; }

    [SerializeField] private bool _affectVisitorMovement = false;

    private Coroutine _timerCoroutine;

    public float AdminServiceMultiplier { get; private set; } = 1f;
    public float DeviceSessionMultiplier { get; private set; } = 1f;
    public float VisitorMovementMultiplier { get; private set; } = 1f;
    public bool IsActive => AdminServiceMultiplier > 1f || DeviceSessionMultiplier > 1f || VisitorMovementMultiplier > 1f;

    public event Action OnChanged;

    private void Awake()
    {
        Current = this;
    }

    private void OnDestroy()
    {
        if (Current == this)
            Current = null;
    }

    public void Activate(float duration, float multiplier)
    {
        multiplier = Mathf.Max(1f, multiplier);
        duration = Mathf.Max(0.1f, duration);

        AdminServiceMultiplier = multiplier;
        DeviceSessionMultiplier = multiplier;
        VisitorMovementMultiplier = _affectVisitorMovement ? multiplier : 1f;

        OnChanged?.Invoke();

        if (_timerCoroutine != null)
            StopCoroutine(_timerCoroutine);

        _timerCoroutine = StartCoroutine(ResetAfter(duration));
    }

    public void ResetEffect()
    {
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }

        AdminServiceMultiplier = 1f;
        DeviceSessionMultiplier = 1f;
        VisitorMovementMultiplier = 1f;

        OnChanged?.Invoke();
    }

    private IEnumerator ResetAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        ResetEffect();
    }
}