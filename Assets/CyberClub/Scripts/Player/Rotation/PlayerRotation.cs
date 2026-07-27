using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRotation : MonoBehaviour
{
    private const string MouseSensitivityKey = "Input_MouseSensitivity";
    private const string TrackpadSensitivityKey = "Input_TrackpadSensitivity";
    private const string MobileSensitivityKey = "Input_MobileSensitivity";
    private const string PreferTrackpadKey = "Input_PreferTrackpad";

    [Header("Camera chain")]
    [SerializeField] private CinemachineInputAxisController _cinemachineInputAxisController;
    [SerializeField] private CinemachineOrbitalFollow _thirdPersonOrbit;
    [SerializeField] private Transform _playerHead;
    [SerializeField] private InteractionWithUI _interactionWithUI;
    [SerializeField] private PlayerInputReader _inputReader;

    [Header("Pointer sensitivity (degrees per pixel)")]
    [SerializeField, Range(0.01f, 1f)] private float _desktopMouseSensitivity = 0.2f;
    [SerializeField, Range(0.01f, 1.5f)] private float _trackpadSensitivity = 0.34f;
    [SerializeField] private bool _preferTrackpadOnMac = true;

    [Header("Mobile sensitivity")]
    [SerializeField, Min(1f)] private float _mobileHorizontalDegreesPerScreen = 240f;
    [SerializeField, Min(1f)] private float _mobileVerticalDegreesPerScreen = 160f;
    [SerializeField, Range(0.25f, 2f)] private float _mobileLookSensitivity = 1f;

    [Header("Gamepad sensitivity")]
    [SerializeField, Min(1f)] private float _gamepadDegreesPerSecond = 140f;

    [Header("Pitch limits")]
    [SerializeField] private float _maxRotationX = 70f;
    [SerializeField] private float _minRotationX = -90f;

    private float _rotationX;
    private float _rotationY;
    private bool _isRotateActive;
    private bool _preferTrackpadSensitivity;
    private bool _settingsDirty;
    private float _saveSettingsAt;

    public float DesktopMouseSensitivity => _desktopMouseSensitivity;
    public float TrackpadSensitivity => _trackpadSensitivity;
    public float MobileLookSensitivity => _mobileLookSensitivity;
    public bool PreferTrackpadSensitivity => _preferTrackpadSensitivity;

    private void Awake()
    {
        if (_inputReader == null)
            _inputReader = GetComponent<PlayerInputReader>();

        if (_thirdPersonOrbit == null && _cinemachineInputAxisController != null)
        {
            _thirdPersonOrbit =
                _cinemachineInputAxisController.GetComponent<CinemachineOrbitalFollow>();
        }

        // The controller reads <Pointer>/delta directly, including the primary touch.
        // PlayerRotation is the single filtered route for mouse, trackpad and touch.
        if (_cinemachineInputAxisController != null)
            _cinemachineInputAxisController.enabled = false;

        _rotationX = _playerHead != null
            ? NormalizeSignedAngle(_playerHead.localEulerAngles.x)
            : 0f;
        _rotationY = NormalizeSignedAngle(transform.localEulerAngles.y);

        LoadSensitivitySettings();
    }

    private void OnEnable()
    {
        if (_interactionWithUI != null)
            _interactionWithUI.IsInteractsChanged += SwitchRotateActive;
    }

    private void OnDisable()
    {
        if (_interactionWithUI != null)
            _interactionWithUI.IsInteractsChanged -= SwitchRotateActive;

        SaveSensitivitySettingsNow();
    }

    private void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
            SaveSensitivitySettingsNow();
    }

    private void OnApplicationQuit()
    {
        SaveSensitivitySettingsNow();
    }

    private void Update()
    {
        if (_isRotateActive)
            Rotate();

        if (_settingsDirty && Time.unscaledTime >= _saveSettingsAt)
            SaveSensitivitySettingsNow();
    }

    public void SetDesktopMouseSensitivity(float value)
    {
        _desktopMouseSensitivity = Mathf.Clamp(value, 0.01f, 1f);
        MarkSettingsDirty();
    }

    public void SetTrackpadSensitivity(float value)
    {
        _trackpadSensitivity = Mathf.Clamp(value, 0.01f, 1.5f);
        MarkSettingsDirty();
    }

    public void SetMobileLookSensitivity(float value)
    {
        _mobileLookSensitivity = Mathf.Clamp(value, 0.25f, 2f);
        MarkSettingsDirty();
    }

    public void SetPreferTrackpadSensitivity(bool value)
    {
        _preferTrackpadSensitivity = value;
        MarkSettingsDirty();
    }

    private void Rotate()
    {
        if (_inputReader == null)
            return;

        Vector2 inputDelta = _inputReader.Look;
        if (inputDelta.sqrMagnitude <= 0f)
            return;

        Vector2 rotationDelta = ConvertInputToDegrees(inputDelta);

        _rotationX = Mathf.Clamp(
            _rotationX - rotationDelta.y,
            _minRotationX,
            _maxRotationX);
        _rotationY += rotationDelta.x;

        if (_playerHead != null)
            _playerHead.localRotation = Quaternion.Euler(_rotationX, 0f, 0f);

        transform.localRotation = Quaternion.Euler(0f, _rotationY, 0f);
        ApplyThirdPersonVerticalOrbit(rotationDelta.y);
    }

    private Vector2 ConvertInputToDegrees(Vector2 inputDelta)
    {
        if (_inputReader.IsTouchMode)
        {
            float width = Mathf.Max(1f, Screen.width);
            float height = Mathf.Max(1f, Screen.height);

            return new Vector2(
                inputDelta.x / width *
                _mobileHorizontalDegreesPerScreen *
                _mobileLookSensitivity,
                inputDelta.y / height *
                _mobileVerticalDegreesPerScreen *
                _mobileLookSensitivity);
        }

        if (_inputReader.IsLookFromGamepad)
            return inputDelta * (_gamepadDegreesPerSecond * Time.deltaTime);

        float sensitivity = ShouldUseTrackpadSensitivity()
            ? _trackpadSensitivity
            : _desktopMouseSensitivity;

        // Pointer delta is already accumulated for this frame.
        return inputDelta * sensitivity;
    }

    private void ApplyThirdPersonVerticalOrbit(float verticalDegrees)
    {
        if (_thirdPersonOrbit == null || Mathf.Approximately(verticalDegrees, 0f))
            return;

        InputAxis axis = _thirdPersonOrbit.VerticalAxis;
        axis.Value = axis.ClampValue(axis.Value - verticalDegrees);
        _thirdPersonOrbit.VerticalAxis = axis;
    }

    private void SwitchRotateActive(bool isInterfaceActive)
    {
        _isRotateActive = !isInterfaceActive;

        if (_cinemachineInputAxisController != null &&
            _cinemachineInputAxisController.enabled)
        {
            _cinemachineInputAxisController.enabled = false;
        }
    }

    private bool ShouldUseTrackpadSensitivity()
    {
        if (_preferTrackpadSensitivity)
            return true;

        string product = Mouse.current?.description.product ?? string.Empty;
        string manufacturer = Mouse.current?.description.manufacturer ?? string.Empty;
        bool deviceLooksLikeTrackpad =
            product.IndexOf("trackpad", StringComparison.OrdinalIgnoreCase) >= 0 ||
            product.IndexOf("touchpad", StringComparison.OrdinalIgnoreCase) >= 0 ||
            manufacturer.IndexOf("trackpad", StringComparison.OrdinalIgnoreCase) >= 0;

        if (deviceLooksLikeTrackpad)
            return true;

        bool isMac =
            Application.platform == RuntimePlatform.OSXEditor ||
            Application.platform == RuntimePlatform.OSXPlayer ||
            SystemInfo.operatingSystem.IndexOf(
                "Mac",
                StringComparison.OrdinalIgnoreCase) >= 0;

        return _preferTrackpadOnMac && isMac;
    }

    private void LoadSensitivitySettings()
    {
        _desktopMouseSensitivity = PlayerPrefs.GetFloat(
            MouseSensitivityKey,
            _desktopMouseSensitivity);
        _trackpadSensitivity = PlayerPrefs.GetFloat(
            TrackpadSensitivityKey,
            _trackpadSensitivity);
        _mobileLookSensitivity = PlayerPrefs.GetFloat(
            MobileSensitivityKey,
            _mobileLookSensitivity);
        _preferTrackpadSensitivity =
            PlayerPrefs.GetInt(PreferTrackpadKey, 0) == 1;
    }

    private void MarkSettingsDirty()
    {
        _settingsDirty = true;
        _saveSettingsAt = Time.unscaledTime + 0.5f;
    }

    private void SaveSensitivitySettingsNow()
    {
        if (!_settingsDirty)
            return;

        PlayerPrefs.SetFloat(MouseSensitivityKey, _desktopMouseSensitivity);
        PlayerPrefs.SetFloat(TrackpadSensitivityKey, _trackpadSensitivity);
        PlayerPrefs.SetFloat(MobileSensitivityKey, _mobileLookSensitivity);
        PlayerPrefs.SetInt(
            PreferTrackpadKey,
            _preferTrackpadSensitivity ? 1 : 0);
        PlayerPrefs.Save();
        _settingsDirty = false;
    }

    private static float NormalizeSignedAngle(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }
}
