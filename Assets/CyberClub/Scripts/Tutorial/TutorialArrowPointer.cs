using UnityEngine;
using UnityEngine.UI;

public class TutorialArrowPointer : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Camera _worldCamera;

    [Header("Arrow")]
    [SerializeField] private RectTransform _arrow;
    [SerializeField] private Vector2 _screenOffset = new Vector2(0f, 80f);
    [SerializeField] private Vector2 _screenPadding = new Vector2(70f, 70f);

    [Header("Visual")]
    [SerializeField] private Vector2 _size = new Vector2(90f, 90f);
    [SerializeField] private bool _forceSize = true;

    private Transform _target;

    private void Awake()
    {
        if (_canvas == null)
            _canvas = GetComponentInParent<Canvas>();

        if (_worldCamera == null)
            _worldCamera = Camera.main;

        PrepareArrow();
        Hide();
    }

    private void LateUpdate()
    {
        if (_target == null)
        {
            Hide();
            return;
        }

        if (_arrow == null)
            return;

        Vector2 screenPoint = GetTargetScreenPoint(_target);
        screenPoint += _screenOffset;

        screenPoint.x = Mathf.Clamp(screenPoint.x, _screenPadding.x, Screen.width - _screenPadding.x);
        screenPoint.y = Mathf.Clamp(screenPoint.y, _screenPadding.y, Screen.height - _screenPadding.y);

        MoveArrowToScreenPoint(screenPoint);
    }

    public void PointTo(Transform target)
    {
        _target = target;
        PrepareArrow();

        if (_arrow != null)
            _arrow.gameObject.SetActive(_target != null);
    }

    public void Hide()
    {
        _target = null;

        if (_arrow != null)
            _arrow.gameObject.SetActive(false);
    }

    private Vector2 GetTargetScreenPoint(Transform target)
    {
        RectTransform rectTarget = target as RectTransform;

        if (rectTarget != null)
            return GetRectTransformScreenPoint(rectTarget);

        if (_worldCamera == null)
            _worldCamera = Camera.main;

        if (_worldCamera == null)
            return Vector2.zero;

        Vector3 screenPoint = _worldCamera.WorldToScreenPoint(target.position);

        if (screenPoint.z < 0f)
        {
            screenPoint.x = Screen.width - screenPoint.x;
            screenPoint.y = Screen.height - screenPoint.y;
        }

        return screenPoint;
    }

    private Vector2 GetRectTransformScreenPoint(RectTransform rectTarget)
    {
        Vector3[] corners = new Vector3[4];
        rectTarget.GetWorldCorners(corners);

        Vector3 worldCenter = (corners[0] + corners[1] + corners[2] + corners[3]) * 0.25f;

        Canvas targetCanvas = rectTarget.GetComponentInParent<Canvas>();
        Camera targetCamera = GetCameraForCanvas(targetCanvas);

        return RectTransformUtility.WorldToScreenPoint(targetCamera, worldCenter);
    }

    private void MoveArrowToScreenPoint(Vector2 screenPoint)
    {
        if (_canvas == null)
            _canvas = GetComponentInParent<Canvas>();

        Camera canvasCamera = GetCameraForCanvas(_canvas);

        RectTransform arrowParent = _arrow.parent as RectTransform;

        if (arrowParent != null)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    arrowParent,
                    screenPoint,
                    canvasCamera,
                    out Vector2 localPoint))
            {
                _arrow.gameObject.SetActive(true);
                _arrow.anchoredPosition = localPoint;
            }

            return;
        }

        _arrow.gameObject.SetActive(true);
        _arrow.position = screenPoint;
    }

    private Camera GetCameraForCanvas(Canvas canvas)
    {
        if (canvas == null)
            return _worldCamera;

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        if (canvas.worldCamera != null)
            return canvas.worldCamera;

        if (_worldCamera != null)
            return _worldCamera;

        return Camera.main;
    }

    private void PrepareArrow()
    {
        if (_arrow == null)
            return;

        if (_forceSize)
            _arrow.sizeDelta = _size;

        Image image = _arrow.GetComponent<Image>();

        if (image != null)
            image.raycastTarget = false;
    }
}
