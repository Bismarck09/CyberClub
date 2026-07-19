using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Canvas))]
public class WorldSpaceCanvasScaleCompensator : MonoBehaviour
{
    [SerializeField, Min(0f)] private float _referenceWorldScale;

    private Transform _cachedTransform;

    private void Awake()
    {
        _cachedTransform = transform;
        CompensateScale(true);
    }

    private void LateUpdate()
    {
        CompensateScale(false);
    }

    private void CompensateScale(bool captureReference)
    {
        if (_cachedTransform == null || _cachedTransform.parent == null)
            return;

        Vector3 lossyScale = _cachedTransform.lossyScale;

        if (captureReference && _referenceWorldScale <= 0f)
        {
            _referenceWorldScale = Mathf.Max(
                0.0001f,
                Mathf.Min(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y), Mathf.Abs(lossyScale.z)));
        }

        if (_referenceWorldScale <= 0f)
            return;

        Vector3 localScale = _cachedTransform.localScale;
        localScale.x *= SafeRatio(_referenceWorldScale, lossyScale.x);
        localScale.y *= SafeRatio(_referenceWorldScale, lossyScale.y);
        localScale.z *= SafeRatio(_referenceWorldScale, lossyScale.z);
        _cachedTransform.localScale = localScale;
    }

    private static float SafeRatio(float desired, float current)
    {
        float magnitude = Mathf.Abs(current);
        return magnitude > 0.000001f ? desired / magnitude : 1f;
    }
}
