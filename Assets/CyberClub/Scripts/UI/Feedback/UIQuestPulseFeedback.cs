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
        _uiRectTransform.DOPunchScale(Vector3.one * 0.05f, 1, 8, 0.7f).SetLoops(-1, LoopType.Restart);
    }
}
