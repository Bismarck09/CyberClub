using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public sealed class MobileLookArea : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler,
    IEndDragHandler,
    ICancelHandler
{
    [SerializeField, Min(0.01f)] private float _sensitivity = 0.12f;

    private const int NoPointer = int.MinValue;

    private PlayerInputReader _inputReader;
    private Canvas _canvas;
    private int _pointerId = NoPointer;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
    }

    public void Bind(PlayerInputReader inputReader)
    {
        _inputReader = inputReader;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_pointerId != NoPointer ||
            _inputReader == null ||
            !_inputReader.IsGameplayInputAvailable ||
            IsCoveredByOtherUI(eventData))
        {
            return;
        }

        _pointerId = eventData.pointerId;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != _pointerId)
            return;

        if (_inputReader == null ||
            !_inputReader.IsGameplayInputAvailable ||
            IsCoveredByOtherUI(eventData))
        {
            ResetControl();
            return;
        }

        float canvasScale = _canvas != null
            ? Mathf.Max(0.01f, _canvas.scaleFactor)
            : GetFallbackScreenScale();

        _inputReader.AddMobileLookDelta(
            eventData.delta * (_sensitivity / canvasScale));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Release(eventData.pointerId);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Release(eventData.pointerId);
    }

    public void OnCancel(BaseEventData unusedEventData)
    {
        ResetControl();
    }

    public void ResetControl()
    {
        _pointerId = NoPointer;
        _inputReader?.ResetMobileLook();
    }

    private void OnDisable()
    {
        ResetControl();
    }

    private void Release(int pointerId)
    {
        if (pointerId == _pointerId)
            ResetControl();
    }

    private bool IsCoveredByOtherUI(PointerEventData eventData)
    {
        GameObject raycastTarget = eventData.pointerCurrentRaycast.gameObject;

        return raycastTarget != null &&
            raycastTarget != gameObject &&
            !raycastTarget.transform.IsChildOf(transform);
    }

    private static float GetFallbackScreenScale()
    {
        if (Screen.width <= 0 || Screen.height <= 0)
            return 1f;

        return Mathf.Max(
            0.01f,
            Mathf.Min(Screen.width / 1920f, Screen.height / 1080f));
    }
}
