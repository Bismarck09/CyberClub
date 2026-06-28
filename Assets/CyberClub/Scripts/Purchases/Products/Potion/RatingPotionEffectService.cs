using System.Collections;
using UnityEngine;

public class RatingPotionEffectService : MonoBehaviour
{
    private Coroutine _timerCoroutine;

    public bool IsActive { get; private set; }
    public int Multiplier { get; private set; } = 1;

    public void Activate(float duration, int multiplier)
    {
        IsActive = true;
        Multiplier = Mathf.Max(1, multiplier);

        if (_timerCoroutine != null)
            StopCoroutine(_timerCoroutine);

        _timerCoroutine = StartCoroutine(ResetAfter(duration));

        Debug.Log($"Зелье рейтинга активировано как заготовка. Логика рейтинга будет добавлена позже. x{Multiplier}, {duration} секунд.");
    }

    private IEnumerator ResetAfter(float duration)
    {
        yield return new WaitForSeconds(Mathf.Max(0.1f, duration));
        ResetEffect();
    }

    public void ResetEffect()
    {
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }

        IsActive = false;
        Multiplier = 1;

        Debug.Log("Зелье рейтинга закончилось.");
    }
}