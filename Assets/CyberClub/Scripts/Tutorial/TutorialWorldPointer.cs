using UnityEngine;

public class TutorialWorldPointer : MonoBehaviour
{
    [SerializeField] private RectTransform _pointer;
    [SerializeField] private Camera _camera;
    [SerializeField] private Vector2 _screenOffset = new Vector2(0f, 70f);
    [SerializeField] private bool _hideWhenNoTarget = true;

    private Transform _target;

    private void Awake()
    {
        if (_camera == null)
            _camera = Camera.main;

        Hide();
    }

    private void LateUpdate()
    {
        if (_target == null)
        {
            if (_hideWhenNoTarget)
                Hide();

            return;
        }

        if (_camera == null)
            _camera = Camera.main;

        if (_camera == null || _pointer == null)
            return;

        Vector3 screenPoint = _camera.WorldToScreenPoint(_target.position);

        bool isBehindCamera = screenPoint.z < 0f;

        if (isBehindCamera)
            screenPoint *= -1f;

        screenPoint.x = Mathf.Clamp(screenPoint.x, 80f, Screen.width - 80f);
        screenPoint.y = Mathf.Clamp(screenPoint.y, 80f, Screen.height - 80f);

        _pointer.gameObject.SetActive(true);
        _pointer.position = screenPoint + (Vector3)_screenOffset;
    }

    public void PointTo(Transform target)
    {
        _target = target;

        if (_pointer != null)
            _pointer.gameObject.SetActive(_target != null);
    }

    public void Hide()
    {
        _target = null;

        if (_pointer != null)
            _pointer.gameObject.SetActive(false);
    }
}