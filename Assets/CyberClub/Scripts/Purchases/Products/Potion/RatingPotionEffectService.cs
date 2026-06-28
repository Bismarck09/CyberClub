using System.Collections;
using UnityEngine;

public class RatingPotionEffectService : MonoBehaviour
{
    private Coroutine _legacyTimerCoroutine;

    public bool IsActive { get; private set; }
    public int Multiplier { get; private set; } = 1;

    public void Apply(int multiplier)
    {
        IsActive = true;
        Multiplier = Mathf.Max(1, multiplier);

        Debug.Log($"Зелье рейтинга применено как заготовка. x{Multiplier}.");
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

        IsActive = false;
        Multiplier = 1;

        Debug.Log("Зелье рейтинга выключено.");
    }
}
