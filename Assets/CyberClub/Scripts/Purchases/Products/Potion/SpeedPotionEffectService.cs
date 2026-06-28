using System;
using System.Collections;
using UnityEngine;

public class SpeedPotionEffectService : MonoBehaviour
{
    public static SpeedPotionEffectService Current { get; private set; }

    [Header("What speed potion affects")]
    [SerializeField] private bool _affectAdminService = true;
    [SerializeField] private bool _affectDeviceSession = true;
    [SerializeField] private bool _affectVisitorMovement = true;

    private Coroutine _legacyTimerCoroutine;

    public float AdminServiceMultiplier { get; private set; } = 1f;
    public float DeviceSessionMultiplier { get; private set; } = 1f;
    public float VisitorMovementMultiplier { get; private set; } = 1f;

    public bool IsActive => AdminServiceMultiplier > 1f || DeviceSessionMultiplier > 1f || VisitorMovementMultiplier > 1f;

    public event Action OnChanged;

    private void Awake()
    {
        Current = this;
    }

    private void OnEnable()
    {
        Current = this;
    }

    private void OnDestroy()
    {
        if (Current == this)
            Current = null;
    }

    public void Apply(int multiplier)
    {
        float safeMultiplier = Mathf.Max(1, multiplier);

        AdminServiceMultiplier = _affectAdminService ? safeMultiplier : 1f;
        DeviceSessionMultiplier = _affectDeviceSession ? safeMultiplier : 1f;
        VisitorMovementMultiplier = _affectVisitorMovement ? safeMultiplier : 1f;

        OnChanged?.Invoke();

        Debug.Log($"Зелье скорости применено: Admin x{AdminServiceMultiplier}, Session x{DeviceSessionMultiplier}, Movement x{VisitorMovementMultiplier}.");
    }

    public void Activate(float duration, int multiplier)
    {
        Apply(multiplier);

        if (_legacyTimerCoroutine != null)
            StopCoroutine(_legacyTimerCoroutine);

        _legacyTimerCoroutine = StartCoroutine(LegacyResetAfter(duration));
    }

    private IEnumerator LegacyResetAfter(float duration)
    {
        yield return new WaitForSeconds(Mathf.Max(0.1f, duration));
        ResetEffect();
        _legacyTimerCoroutine = null;
    }

    public void ResetEffect()
    {
        if (_legacyTimerCoroutine != null)
        {
            StopCoroutine(_legacyTimerCoroutine);
            _legacyTimerCoroutine = null;
        }

        AdminServiceMultiplier = 1f;
        DeviceSessionMultiplier = 1f;
        VisitorMovementMultiplier = 1f;

        OnChanged?.Invoke();

        Debug.Log("Зелье скорости выключено.");
    }
}
