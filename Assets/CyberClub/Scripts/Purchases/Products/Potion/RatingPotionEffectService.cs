using System;
using System.Collections;
using UnityEngine;

public class RatingPotionEffectService : MonoBehaviour
{
    private Coroutine _timerCoroutine;

    public bool IsActive { get; private set; }
    public float Multiplier { get; private set; } = 1f;

    public event Action OnChanged;

    public void Activate(float duration, float multiplier)
    {
        // Заготовка под будущий рейтинг.
        // Сейчас зелье только включается на время, чтобы магазин уже работал.
        IsActive = true;
        Multiplier = Mathf.Max(1f, multiplier);

        OnChanged?.Invoke();

        if (_timerCoroutine != null)
            StopCoroutine(_timerCoroutine);

        _timerCoroutine = StartCoroutine(ResetAfter(Mathf.Max(0.1f, duration)));
    }

    public void ResetEffect()
    {
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }

        IsActive = false;
        Multiplier = 1f;

        OnChanged?.Invoke();
    }

    private IEnumerator ResetAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        ResetEffect();
    }
}