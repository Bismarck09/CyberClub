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
    [SerializeField] private bool _rotateAtScreenEdge = true;

    [Header("Visual")]
    [SerializeField] private Vector2 _size = new Vector2(90f, 90f);
    [SerializeField] private bool _forceSize = true;

    private Transform _target;
    private readonly Vector3[] _targetCorners = new Vector3[4];

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

        if (!TryGetTargetScreenPoint(_target, out Vector2 targetPoint, out bool isBehindCamera))
        {
            _arrow.gameObject.SetActive(false);
            return;
        }

        Rect safeRect = GetPaddedSafeArea();
        Vector2 screenCenter = safeRect.center;

        if (isBehindCamera)
            targetPoint = screenCenter - (targetPoint - screenCenter);

        Vector2 requestedPoint = targetPoint + _screenOffset;
        bool isOnScreen = !isBehindCamera && safeRect.Contains(requestedPoint);
        Vector2 finalPoint = isOnScreen
            ? requestedPoint
            : new Vector2(
                Mathf.Clamp(requestedPoint.x, safeRect.xMin, safeRect.xMax),
                Mathf.Clamp(requestedPoint.y, safeRect.yMin, safeRect.yMax));

        if (_rotateAtScreenEdge && !isOnScreen)
        {
            Vector2 direction = targetPoint - screenCenter;

            if (direction.sqrMagnitude > 0.001f)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                _arrow.localRotation = Quaternion.Euler(0f, 0f, angle);
            }
        }
        else
        {
            _arrow.localRotation = Quaternion.identity;
        }

        MoveArrowToScreenPoint(finalPoint);
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
        {
            _arrow.localRotation = Quaternion.identity;
            _arrow.gameObject.SetActive(false);
        }
    }

    private bool TryGetTargetScreenPoint(Transform target, out Vector2 screenPoint, out bool isBehindCamera)
    {
        screenPoint = Vector2.zero;
        isBehindCamera = false;
        RectTransform rectTarget = target as RectTransform;

        if (rectTarget != null)
        {
            screenPoint = GetRectTransformScreenPoint(rectTarget);
            return true;
        }

        if (_worldCamera == null)
            _worldCamera = Camera.main;

        if (_worldCamera == null)
            return false;

        Vector3 projected = _worldCamera.WorldToScreenPoint(target.position);
        screenPoint = projected;
        isBehindCamera = projected.z <= 0f;
        return true;
    }

    private Vector2 GetRectTransformScreenPoint(RectTransform rectTarget)
    {
        rectTarget.GetWorldCorners(_targetCorners);

        Vector3 worldCenter = (_targetCorners[0] + _targetCorners[1] + _targetCorners[2] + _targetCorners[3]) * 0.25f;

        Canvas targetCanvas = rectTarget.GetComponentInParent<Canvas>();
        Camera targetCamera = GetCameraForCanvas(targetCanvas);

        return RectTransformUtility.WorldToScreenPoint(targetCamera, worldCenter);
    }

    private Rect GetPaddedSafeArea()
    {
        Rect safeArea = Screen.safeArea;
        float horizontalPadding = Mathf.Min(_screenPadding.x, safeArea.width * 0.45f);
        float verticalPadding = Mathf.Min(_screenPadding.y, safeArea.height * 0.45f);

        return new Rect(
            safeArea.xMin + horizontalPadding,
            safeArea.yMin + verticalPadding,
            Mathf.Max(1f, safeArea.width - horizontalPadding * 2f),
            Mathf.Max(1f, safeArea.height - verticalPadding * 2f));
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
