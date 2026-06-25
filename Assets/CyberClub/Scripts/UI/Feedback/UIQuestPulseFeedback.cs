using System;
using UnityEngine;
using DG.Tweening;

public class UIQuestPulseFeedback : MonoBehaviour
{
    private RectTransform _uiRectTransform;

    public void Awake()
    {
        _uiRectTransform = GetComponent<RectTransform>();
    }

    public void ActivatePulse()
    {
        _uiRectTransform.DOScale(Vector3.one * 1.04f, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }
}
