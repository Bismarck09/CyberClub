using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerInputReader))]
public sealed class MobileControlsHUD : MonoBehaviour
{
    [SerializeField] private GameObject _visualRoot;
    [SerializeField] private MobileVirtualJoystick _movementJoystick;
    [SerializeField] private MobileLookArea _lookArea;
    [SerializeField] private RectTransform _safeAreaRoot;

    private PlayerInputReader _inputReader;
    private Rect _lastSafeArea;

    private void Awake()
    {
        _inputReader = GetComponent<PlayerInputReader>();

        ValidateReferences();
        _movementJoystick?.Bind(_inputReader);
        _lookArea?.Bind(_inputReader, _movementJoystick);
        ApplySafeArea();
        SetHudVisible(_inputReader != null && _inputReader.IsTouchMode);
    }

    private void OnEnable()
    {
        if (_inputReader == null)
            return;

        _inputReader.OnControlModeChanged += SetHudVisible;
        _inputReader.OnMobileControlsResetRequested += ResetControlViews;
    }

    private void OnDisable()
    {
        if (_inputReader != null)
        {
            _inputReader.OnControlModeChanged -= SetHudVisible;
            _inputReader.OnMobileControlsResetRequested -= ResetControlViews;
        }

        ResetControls();
    }

    private void Update()
    {
        if (_safeAreaRoot != null && _lastSafeArea != Screen.safeArea)
            ApplySafeArea();
    }

    private void SetHudVisible(bool isTouchMode)
    {
        if (_visualRoot == null)
            return;

        if (!isTouchMode)
            ResetControls();

        if (_visualRoot.activeSelf != isTouchMode)
            _visualRoot.SetActive(isTouchMode);
    }

    private void ResetControlViews()
    {
        _movementJoystick?.ResetControl();
        _lookArea?.ResetControl();
    }

    private void ResetControls()
    {
        ResetControlViews();
        _inputReader?.ResetMobileState();
    }

    private void ApplySafeArea()
    {
        if (_safeAreaRoot == null || Screen.width <= 0 || Screen.height <= 0)
            return;

        Rect safeArea = Screen.safeArea;
        _lastSafeArea = safeArea;
        _safeAreaRoot.anchorMin = new Vector2(
            safeArea.xMin / Screen.width,
            safeArea.yMin / Screen.height);
        _safeAreaRoot.anchorMax = new Vector2(
            safeArea.xMax / Screen.width,
            safeArea.yMax / Screen.height);
        _safeAreaRoot.offsetMin = Vector2.zero;
        _safeAreaRoot.offsetMax = Vector2.zero;
    }

    private void ValidateReferences()
    {
        if (_visualRoot == null)
            ReportMissing(nameof(_visualRoot));
        if (_movementJoystick == null)
            ReportMissing(nameof(_movementJoystick));
        if (_lookArea == null)
            ReportMissing(nameof(_lookArea));
    }

    private void ReportMissing(string fieldName)
    {
        Debug.LogError(
            $"MobileControlsHUD: поле {fieldName} не назначено на GameObject '{name}'.",
            this);
    }
}
