using UnityEngine;
using TMPro;
using DG.Tweening;

public class FloatingResourceText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private CanvasGroup _canvasGroup;

    public void Play(int amount, Color color)
    {
        _text.text = amount > 0 ? $"+{amount}" : amount.ToString();
        _text.color = color;

        transform.localScale = Vector3.zero;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));

        sequence.Join(
            transform.DOMoveY(transform.position.y + 80f, 1f)
                .SetEase(Ease.OutQuad)
        );

        sequence.Join(
            _canvasGroup.DOFade(0f, 1f)
        );

        sequence.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
}
