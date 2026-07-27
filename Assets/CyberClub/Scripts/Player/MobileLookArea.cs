using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class MobileLookArea : MonoBehaviour
{
    [SerializeField] private Rect _activationArea = new(0.4f, 0f, 0.6f, 1f);

    private const int NoPointer = int.MinValue;

    private PlayerInputReader _inputReader;
    private MobileVirtualJoystick _movementJoystick;
    private TouchControl _activeTouch;
    private int _pointerId = NoPointer;

    private void Awake()
    {
        Graphic[] graphics = GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
            graphics[i].raycastTarget = false;
    }

    private void Update()
    {
        if (_activeTouch != null)
        {
            UpdateCapturedTouch();
            return;
        }

        if (_inputReader == null ||
            !_inputReader.IsTouchMode ||
            !_inputReader.IsGameplayInputAvailable)
        {
            return;
        }

        TryCaptureNewTouch();
    }

    public void Bind(
        PlayerInputReader inputReader,
        MobileVirtualJoystick movementJoystick)
    {
        _inputReader = inputReader;
        _movementJoystick = movementJoystick;
    }

    public void ResetControl()
    {
        _activeTouch = null;
        _pointerId = NoPointer;
        _inputReader?.ResetMobileLook();
    }

    private void OnDisable()
    {
        ResetControl();
    }

    private void TryCaptureNewTouch()
    {
        Touchscreen touchscreen = Touchscreen.current;
        if (touchscreen == null)
            return;

        foreach (TouchControl touch in touchscreen.touches)
        {
            if (!touch.press.wasPressedThisFrame)
                continue;

            int pointerId = touch.touchId.ReadValue();
            Vector2 screenPosition = touch.position.ReadValue();

            if (!GetActivationScreenRect().Contains(screenPosition) ||
                (_movementJoystick != null &&
                 _movementJoystick.IsPointerCaptured(pointerId)) ||
                MobilePointerUiGuard.IsPointerOverInteractiveUi(
                    screenPosition,
                    pointerId,
                    transform))
            {
                continue;
            }

            _activeTouch = touch;
            _pointerId = pointerId;
            return;
        }
    }

    private void UpdateCapturedTouch()
    {
        if (_inputReader == null ||
            !_inputReader.IsGameplayInputAvailable ||
            !_activeTouch.press.isPressed ||
            (_movementJoystick != null &&
             _movementJoystick.IsPointerCaptured(_pointerId)))
        {
            ResetControl();
            return;
        }

        Vector2 delta = _activeTouch.delta.ReadValue();
        if (delta.sqrMagnitude > 0f)
            _inputReader.AddMobileLookDelta(delta);
    }

    private Rect GetActivationScreenRect()
    {
        float xMin = Mathf.Clamp01(_activationArea.xMin) * Screen.width;
        float yMin = Mathf.Clamp01(_activationArea.yMin) * Screen.height;
        float xMax = Mathf.Clamp01(_activationArea.xMax) * Screen.width;
        float yMax = Mathf.Clamp01(_activationArea.yMax) * Screen.height;

        Rect configured = Rect.MinMaxRect(
            Mathf.Min(xMin, xMax),
            Mathf.Min(yMin, yMax),
            Mathf.Max(xMin, xMax),
            Mathf.Max(yMin, yMax));
        Rect safeArea = Screen.safeArea;

        float safeXMin = Mathf.Max(configured.xMin, safeArea.xMin);
        float safeYMin = Mathf.Max(configured.yMin, safeArea.yMin);
        float safeXMax = Mathf.Min(configured.xMax, safeArea.xMax);
        float safeYMax = Mathf.Min(configured.yMax, safeArea.yMax);

        return safeXMax > safeXMin && safeYMax > safeYMin
            ? Rect.MinMaxRect(safeXMin, safeYMin, safeXMax, safeYMax)
            : configured;
    }
}
