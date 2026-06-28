using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class RatingUIEffect : MonoBehaviour
{
    [SerializeField] private RatingData _ratingData;
    [SerializeField] private RectTransform _ratingPanel;
    [SerializeField] private Image _ratingIcon;

    private void Start()
    {
        Refresh(0f);
    }

    private void OnEnable()
    {
        if (_ratingData != null)
            _ratingData.OnRatingChanged += Refresh;
    }

    private void OnDisable()
    {
        if (_ratingData != null)
            _ratingData.OnRatingChanged -= Refresh;

        if (_ratingPanel != null)
            _ratingPanel.DOKill();
    }

    private void Refresh(float amount)
    {
        if (_ratingData == null || _ratingIcon == null)
            return;

        _ratingIcon.fillAmount = _ratingData.NormalizedRating;

        if (!Mathf.Approximately(amount, 0f))
            PlayRotationEffect(amount);
    }

    private void PlayRotationEffect(float amount)
    {
        if (_ratingPanel == null)
            return;

        _ratingPanel.DOKill();

        float rotation = amount >= 0 ? -8f : 8f;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(_ratingPanel.DORotate(new Vector3(0, 0, rotation), 0.15f).SetEase(Ease.OutQuad));
        sequence.Join(_ratingPanel.DOScale(1.08f, 0.15f));
        sequence.Append(_ratingPanel.DORotate(Vector3.zero, 0.2f).SetEase(Ease.OutBack));
        sequence.Join(_ratingPanel.DOScale(1f, 0.2f));
    }
}
