using UnityEngine;
using DG.Tweening;

public class UIQuestPulseFeedback : MonoBehaviour
{
    private RectTransform _uiRectTransform;
    private Tween _pulseTween;
    private Vector3 _startScale;

    private void Awake()
    {
        _uiRectTransform = GetComponent<RectTransform>();
        _startScale = _uiRectTransform.localScale;
    }

    public void ActivatePulse()
    {
        if (_uiRectTransform == null)
            return;

        _pulseTween?.Kill();

        _uiRectTransform.localScale = _startScale;

        _pulseTween = _uiRectTransform
            .DOScale(_startScale * 1.04f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetTarget(_uiRectTransform);
    }

    public void StopPulse()
    {
        _pulseTween?.Kill();
        _pulseTween = null;

        if (_uiRectTransform != null)
            _uiRectTransform.localScale = _startScale;
    }

    private void OnDisable()
    {
        StopPulse();
    }

    private void OnDestroy()
    {
        _pulseTween?.Kill();
    }
}