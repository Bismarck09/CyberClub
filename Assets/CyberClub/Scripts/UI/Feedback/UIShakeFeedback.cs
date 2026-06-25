using UnityEngine;
using DG.Tweening;

public class UIShakeFeedback : MonoBehaviour
{
    [SerializeField] private RectTransform _uiRectTransform;
    [SerializeField] private float _shakeDuration;
    [SerializeField] private Vector3 _shakeStrength;

    public void Shake()
    {
        _uiRectTransform.DOShakePosition(_shakeDuration, _shakeStrength);
    }
}
