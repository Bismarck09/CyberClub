using UnityEngine;
using UnityEngine.UI;

public class TutorialWorldPointer : MonoBehaviour
{
    [SerializeField] private RectTransform _pointer;
    [SerializeField] private Camera _camera;
    [SerializeField] private Vector2 _screenOffset = new Vector2(0f, 70f);

    [Header("Visual")]
    [SerializeField] private Vector2 _pointerSize = new Vector2(90f, 90f);
    [SerializeField] private bool _forcePointerSize = true;

    private Transform _target;

    private void Awake()
    {
        if (_camera == null)
            _camera = Camera.main;

        PreparePointer();
        Hide();
    }

    private void LateUpdate()
    {
        if (_target == null)
        {
            Hide();
            return;
        }

        if (_pointer == null)
            return;

        Vector3 screenPoint = GetTargetScreenPoint(_target);

        screenPoint.x = Mathf.Clamp(screenPoint.x, 80f, Screen.width - 80f);
        screenPoint.y = Mathf.Clamp(screenPoint.y, 80f, Screen.height - 80f);

        _pointer.gameObject.SetActive(true);
        _pointer.position = screenPoint + (Vector3)_screenOffset;
    }

    public void PointTo(Transform target)
    {
        _target = target;

        PreparePointer();

        if (_pointer != null)
            _pointer.gameObject.SetActive(_target != null);
    }

    public void Hide()
    {
        _target = null;

        if (_pointer != null)
            _pointer.gameObject.SetActive(false);
    }

    private Vector3 GetTargetScreenPoint(Transform target)
    {
        if (target is RectTransform rectTransform)
            return GetRectTransformScreenPoint(rectTransform);

        if (_camera == null)
            _camera = Camera.main;

        if (_camera == null)
            return Vector3.zero;

        Vector3 point = _camera.WorldToScreenPoint(target.position);

        if (point.z < 0f)
            point *= -1f;

        return point;
    }

    private Vector3 GetRectTransformScreenPoint(RectTransform rectTransform)
    {
        Canvas canvas = rectTransform.GetComponentInParent<Canvas>();

        if (canvas == null)
            return rectTransform.position;

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return rectTransform.position;

        Camera eventCamera = canvas.worldCamera != null ? canvas.worldCamera : _camera;
        return RectTransformUtility.WorldToScreenPoint(eventCamera, rectTransform.position);
    }

    private void PreparePointer()
    {
        if (_pointer == null)
            return;

        if (_forcePointerSize)
            _pointer.sizeDelta = _pointerSize;

        Image image = _pointer.GetComponent<Image>();

        if (image != null)
            image.raycastTarget = false;
    }
}
