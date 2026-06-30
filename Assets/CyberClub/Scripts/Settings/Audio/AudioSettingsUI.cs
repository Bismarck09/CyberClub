using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [SerializeField] private AudioSettingsService _audioSettingsService;

    [Header("Toggles")]
    [SerializeField] private Toggle _musicToggle;
    [SerializeField] private Toggle _effectsToggle;

    [Header("Sliders")]
    [SerializeField] private Slider _musicVolumeSlider;
    [SerializeField] private Slider _effectsVolumeSlider;

    private void Awake()
    {
        if (_audioSettingsService == null)
            _audioSettingsService = FindFirstObjectByType<AudioSettingsService>();
    }

    private void OnEnable()
    {
        if (_audioSettingsService == null)
            _audioSettingsService = FindFirstObjectByType<AudioSettingsService>();

        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void Refresh()
    {
        if (_audioSettingsService == null)
            return;

        if (_musicToggle != null)
            _musicToggle.SetIsOnWithoutNotify(_audioSettingsService.IsMusicEnabled);

        if (_effectsToggle != null)
            _effectsToggle.SetIsOnWithoutNotify(_audioSettingsService.AreEffectsEnabled);

        if (_musicVolumeSlider != null)
            _musicVolumeSlider.SetValueWithoutNotify(_audioSettingsService.MusicVolume);

        if (_effectsVolumeSlider != null)
            _effectsVolumeSlider.SetValueWithoutNotify(_audioSettingsService.EffectsVolume);
    }

    private void Subscribe()
    {
        if (_musicToggle != null)
            _musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);

        if (_effectsToggle != null)
            _effectsToggle.onValueChanged.AddListener(OnEffectsToggleChanged);

        if (_musicVolumeSlider != null)
            _musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (_effectsVolumeSlider != null)
            _effectsVolumeSlider.onValueChanged.AddListener(OnEffectsVolumeChanged);

        if (_audioSettingsService != null)
            _audioSettingsService.OnSettingsChanged += Refresh;
    }

    private void Unsubscribe()
    {
        if (_musicToggle != null)
            _musicToggle.onValueChanged.RemoveListener(OnMusicToggleChanged);

        if (_effectsToggle != null)
            _effectsToggle.onValueChanged.RemoveListener(OnEffectsToggleChanged);

        if (_musicVolumeSlider != null)
            _musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);

        if (_effectsVolumeSlider != null)
            _effectsVolumeSlider.onValueChanged.RemoveListener(OnEffectsVolumeChanged);

        if (_audioSettingsService != null)
            _audioSettingsService.OnSettingsChanged -= Refresh;
    }

    private void OnMusicToggleChanged(bool value)
    {
        if (_audioSettingsService != null)
            _audioSettingsService.SetMusicEnabled(value);
    }

    private void OnEffectsToggleChanged(bool value)
    {
        if (_audioSettingsService != null)
            _audioSettingsService.SetEffectsEnabled(value);
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (_audioSettingsService != null)
            _audioSettingsService.SetMusicVolume(value);
    }

    private void OnEffectsVolumeChanged(float value)
    {
        if (_audioSettingsService != null)
            _audioSettingsService.SetEffectsVolume(value);
    }
}
