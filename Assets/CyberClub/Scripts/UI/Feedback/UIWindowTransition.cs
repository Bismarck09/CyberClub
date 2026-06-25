using UnityEngine;
using DG.Tweening;

public class UIWindowTransition : MonoBehaviour
{
    [SerializeField] private RectTransform _uiRectTransform;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _transitionDuration;

    private Sequence _transitionSequence;

    private Vector3 _startScale;
    private Vector2 _startPosition;

    private void Awake()
    {
        _startScale = _uiRectTransform.localScale;
        _startPosition = _uiRectTransform.anchoredPosition;
        
    }

    private void OnEnable()
    {
        Transition();
    }
    
    public void Transition()
    {
        _canvasGroup.alpha = 0;
        _uiRectTransform.localScale = Vector3.zero;
        _uiRectTransform.anchoredPosition = _startPosition + new Vector2(0, -Screen.height);

        _transitionSequence = DOTween.Sequence();

        _transitionSequence.Append(_uiRectTransform.DOScale(_startScale, _transitionDuration).SetEase(Ease.OutBack));
        _transitionSequence.Join(_uiRectTransform.DOAnchorPos(_startPosition, _transitionDuration).SetEase(Ease.OutBack));
        _transitionSequence.Join(_canvasGroup.DOFade(1, _transitionDuration));
    }
}
