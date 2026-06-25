using System;
using UnityEngine;
using DG.Tweening;


public class UIPopFeedback : MonoBehaviour
{
    private RectTransform _uiRectTransform;

    private void Awake()
    {
        _uiRectTransform = GetComponent<RectTransform>();
    }

    public void ActivatePop()
    {
        _uiRectTransform.DOPunchScale(Vector3.one * 0.05f, 0.25f, 8, 0.7f).SetEase(Ease.InOutCirc);
    }
}
