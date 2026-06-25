using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIHoverFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform _uiRectTransform;
    [SerializeField] private float _hoverScale;
    [SerializeField] private float _hoverDuration;

    private Vector3 _startScale;

    private void Awake()
    {
        _startScale = _uiRectTransform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _uiRectTransform.DOScale(_startScale * _hoverScale, _hoverDuration).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _uiRectTransform.DOScale(_startScale, _hoverDuration).SetEase(Ease.OutQuad);
    }
}
