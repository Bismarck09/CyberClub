using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;


public class RatingUIEffect : MonoBehaviour
{
    [SerializeField] private RatingData _ratingData;
    [SerializeField] private RectTransform _ratingPanel;
    [SerializeField] private Image _ratingIcon;

    private void OnEnable()
    {
        _ratingData.OnRatingChanged += OnRatingChanged;
    }

    private void OnDisable()
    {
        _ratingData.OnRatingChanged -= OnRatingChanged;
    }

    private void OnRatingChanged(float amount)
    {
        _ratingIcon.fillAmount = _ratingData.CurrentRating / 5f;
        PlayRotationEffect(amount);
    }

    private void PlayRotationEffect(float amount)
    {
        _ratingPanel.DOKill();

        float rotation = amount >= 0 ? -8f : 8f;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(
            _ratingPanel
                .DORotate(new Vector3(0, 0, rotation), 0.15f)
                .SetEase(Ease.OutQuad)
        );

        sequence.Join(
            _ratingPanel
                .DOScale(1.08f, 0.15f)
        );

        sequence.Append(
            _ratingPanel
                .DORotate(Vector3.zero, 0.2f)
                .SetEase(Ease.OutBack)
        );

        sequence.Join(
            _ratingPanel
                .DOScale(1f, 0.2f)
        );
    }
}
