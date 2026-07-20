using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public sealed class MobileVirtualJoystick : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler,
    IEndDragHandler,
    ICancelHandler
{
    [SerializeField] private RectTransform _background;
    [SerializeField] private RectTransform _handle;

    private const int NoPointer = int.MinValue;

    private PlayerInputReader _inputReader;
    private int _pointerId = NoPointer;

    private void Awake()
    {
        if (_background == null)
            ReportMissing(nameof(_background));
        if (_handle == null)
            ReportMissing(nameof(_handle));
    }

    public void Bind(PlayerInputReader inputReader)
    {
        _inputReader = inputReader;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_pointerId != NoPointer ||
            _inputReader == null ||
            !_inputReader.IsGameplayInputAvailable)
        {
            return;
        }

        _pointerId = eventData.pointerId;
        UpdateValue(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != _pointerId)
            return;

        if (_inputReader == null || !_inputReader.IsGameplayInputAvailable)
        {
            ResetControl();
            return;
        }

        UpdateValue(eventData);
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

        if (_handle != null)
            _handle.anchoredPosition = Vector2.zero;

        _inputReader?.ResetMobileMovement();
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

    private void UpdateValue(PointerEventData eventData)
    {
        if (_background == null || _handle == null)
            return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _background,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
        {
            return;
        }

        float radius = Mathf.Max(
            1f,
            Mathf.Min(_background.rect.width, _background.rect.height) * 0.5f);
        Vector2 value = Vector2.ClampMagnitude(
            (localPoint - _background.rect.center) / radius,
            1f);

        _handle.anchoredPosition = value * radius * 0.62f;
        _inputReader?.SetMobileMovement(value);
    }

    private void ReportMissing(string fieldName)
    {
        Debug.LogError(
            $"MobileVirtualJoystick: поле {fieldName} не назначено на GameObject '{name}'.",
            this);
    }
}
