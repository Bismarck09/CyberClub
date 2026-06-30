using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioSettingsService : MonoBehaviour
{
    private const int CurrentSettingsVersion = 2;

    private const string SettingsVersionKey = "Audio_SettingsVersion";
    private const string MusicEnabledKey = "Audio_MusicEnabled";
    private const string EffectsEnabledKey = "Audio_EffectsEnabled";
    private const string MusicVolumeKey = "Audio_MusicVolume";
    private const string EffectsVolumeKey = "Audio_EffectsVolume";

    [Header("Sources")]
    [SerializeField] private List<AudioSource> _musicSources = new();
    [SerializeField] private List<AudioSource> _effectsSources = new();

    [Header("Defaults")]
    [Range(0f, 1f)]
    [SerializeField] private float _defaultMusicVolume = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float _defaultEffectsVolume = 0.5f;
    [SerializeField] private bool _defaultMusicEnabled = true;
    [SerializeField] private bool _defaultEffectsEnabled = true;

    public bool IsMusicEnabled { get; private set; }
    public bool AreEffectsEnabled { get; private set; }
    public float MusicVolume { get; private set; }
    public float EffectsVolume { get; private set; }

    public event Action OnSettingsChanged;

    private void Awake()
    {
        Load();
        Apply();
    }

    public void SetMusicEnabled(bool value)
    {
        IsMusicEnabled = value;
        SaveAndApply();
    }

    public void SetEffectsEnabled(bool value)
    {
        AreEffectsEnabled = value;
        SaveAndApply();
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        SaveAndApply();
    }

    public void SetEffectsVolume(float value)
    {
        EffectsVolume = Mathf.Clamp01(value);
        SaveAndApply();
    }

    [ContextMenu("Reset Audio Settings To Defaults")]
    public void ResetToDefaults()
    {
        IsMusicEnabled = _defaultMusicEnabled;
        AreEffectsEnabled = _defaultEffectsEnabled;
        MusicVolume = Mathf.Clamp01(_defaultMusicVolume);
        EffectsVolume = Mathf.Clamp01(_defaultEffectsVolume);

        SaveAndApply();
    }

    public void RegisterMusicSource(AudioSource source)
    {
        if (source == null || _musicSources.Contains(source))
            return;

        _musicSources.Add(source);
        ApplyMusicSource(source);
    }

    public void RegisterEffectsSource(AudioSource source)
    {
        if (source == null || _effectsSources.Contains(source))
            return;

        _effectsSources.Add(source);
        ApplyEffectsSource(source);
    }

    private void Load()
    {
        int savedVersion = PlayerPrefs.GetInt(SettingsVersionKey, 0);

        if (savedVersion < CurrentSettingsVersion)
        {
            IsMusicEnabled = _defaultMusicEnabled;
            AreEffectsEnabled = _defaultEffectsEnabled;
            MusicVolume = Mathf.Clamp01(_defaultMusicVolume);
            EffectsVolume = Mathf.Clamp01(_defaultEffectsVolume);

            Save();
            return;
        }

        IsMusicEnabled = PlayerPrefs.GetInt(MusicEnabledKey, _defaultMusicEnabled ? 1 : 0) == 1;
        AreEffectsEnabled = PlayerPrefs.GetInt(EffectsEnabledKey, _defaultEffectsEnabled ? 1 : 0) == 1;
        MusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, _defaultMusicVolume);
        EffectsVolume = PlayerPrefs.GetFloat(EffectsVolumeKey, _defaultEffectsVolume);
    }

    private void Save()
    {
        PlayerPrefs.SetInt(SettingsVersionKey, CurrentSettingsVersion);
        PlayerPrefs.SetInt(MusicEnabledKey, IsMusicEnabled ? 1 : 0);
        PlayerPrefs.SetInt(EffectsEnabledKey, AreEffectsEnabled ? 1 : 0);
        PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);
        PlayerPrefs.SetFloat(EffectsVolumeKey, EffectsVolume);
        PlayerPrefs.Save();
    }

    private void SaveAndApply()
    {
        Save();
        Apply();
    }

    private void Apply()
    {
        foreach (AudioSource source in _musicSources)
            ApplyMusicSource(source);

        foreach (AudioSource source in _effectsSources)
            ApplyEffectsSource(source);

        OnSettingsChanged?.Invoke();
    }

    private void ApplyMusicSource(AudioSource source)
    {
        if (source == null)
            return;

        source.mute = !IsMusicEnabled;
        source.volume = IsMusicEnabled ? MusicVolume : 0f;
    }

    private void ApplyEffectsSource(AudioSource source)
    {
        if (source == null)
            return;

        source.mute = !AreEffectsEnabled;
        source.volume = AreEffectsEnabled ? EffectsVolume : 0f;
    }
}
