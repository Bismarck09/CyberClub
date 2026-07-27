using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class LookSensitivitySettingsUI : MonoBehaviour
{
    [SerializeField] private PlayerRotation _playerRotation;

    [Header("Authored controls")]
    [SerializeField] private Slider _desktopMouseSensitivitySlider;
    [SerializeField] private Slider _trackpadSensitivitySlider;
    [SerializeField] private Slider _mobileLookSensitivitySlider;
    [SerializeField] private Toggle _preferTrackpadToggle;

    private void OnEnable()
    {
        Refresh();
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void Refresh()
    {
        if (_playerRotation == null)
            return;

        _desktopMouseSensitivitySlider?.SetValueWithoutNotify(
            _playerRotation.DesktopMouseSensitivity);
        _trackpadSensitivitySlider?.SetValueWithoutNotify(
            _playerRotation.TrackpadSensitivity);
        _mobileLookSensitivitySlider?.SetValueWithoutNotify(
            _playerRotation.MobileLookSensitivity);
        _preferTrackpadToggle?.SetIsOnWithoutNotify(
            _playerRotation.PreferTrackpadSensitivity);
    }

    private void Subscribe()
    {
        if (_desktopMouseSensitivitySlider != null)
        {
            _desktopMouseSensitivitySlider.onValueChanged.AddListener(
                SetDesktopMouseSensitivity);
        }

        if (_trackpadSensitivitySlider != null)
        {
            _trackpadSensitivitySlider.onValueChanged.AddListener(
                SetTrackpadSensitivity);
        }

        if (_mobileLookSensitivitySlider != null)
        {
            _mobileLookSensitivitySlider.onValueChanged.AddListener(
                SetMobileLookSensitivity);
        }

        if (_preferTrackpadToggle != null)
            _preferTrackpadToggle.onValueChanged.AddListener(SetPreferTrackpad);
    }

    private void Unsubscribe()
    {
        if (_desktopMouseSensitivitySlider != null)
        {
            _desktopMouseSensitivitySlider.onValueChanged.RemoveListener(
                SetDesktopMouseSensitivity);
        }

        if (_trackpadSensitivitySlider != null)
        {
            _trackpadSensitivitySlider.onValueChanged.RemoveListener(
                SetTrackpadSensitivity);
        }

        if (_mobileLookSensitivitySlider != null)
        {
            _mobileLookSensitivitySlider.onValueChanged.RemoveListener(
                SetMobileLookSensitivity);
        }

        if (_preferTrackpadToggle != null)
            _preferTrackpadToggle.onValueChanged.RemoveListener(SetPreferTrackpad);
    }

    private void SetDesktopMouseSensitivity(float value)
    {
        _playerRotation?.SetDesktopMouseSensitivity(value);
    }

    private void SetTrackpadSensitivity(float value)
    {
        _playerRotation?.SetTrackpadSensitivity(value);
    }

    private void SetMobileLookSensitivity(float value)
    {
        _playerRotation?.SetMobileLookSensitivity(value);
    }

    private void SetPreferTrackpad(bool value)
    {
        _playerRotation?.SetPreferTrackpadSensitivity(value);
    }
}
