using UnityEngine;
using TMPro;
using DG.Tweening;

public class FloatingResourceText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private CanvasGroup _canvasGroup;

    public void Play(int amount, Color color)
    {
        if (_text != null)
        {
            _text.text = ResourceValueFormatter.FormatSigned(amount);
            _text.color = color;
        }

        transform.localScale = Vector3.zero;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
        sequence.Join(transform.DOMoveY(transform.position.y + 80f, 1f).SetEase(Ease.OutQuad));

        if (_canvasGroup != null)
            sequence.Join(_canvasGroup.DOFade(0f, 1f));

        sequence.OnComplete(() => Destroy(gameObject));
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}