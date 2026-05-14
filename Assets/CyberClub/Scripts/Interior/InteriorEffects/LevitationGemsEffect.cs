using UnityEngine;
using DG.Tweening;

public class LevitationGemsEffect : MonoBehaviour
{
    [SerializeField] private Transform[] gems;
    [SerializeField] private float levitationHeight = 0.5f;
    [SerializeField] private float levitationDuration = 2f;

    void Start()
    {
        StartEffect();
    }

    private void StartEffect()
    {
        transform.DOMoveY(transform.position.y + levitationHeight, levitationDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        transform.DORotate(new Vector3(0, 360, 0), levitationDuration * 2, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
}
