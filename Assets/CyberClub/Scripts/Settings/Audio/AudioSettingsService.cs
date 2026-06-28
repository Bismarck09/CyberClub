using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioSettingsService : MonoBehaviour
{
    private const string MusicEnabledKey = "Audio_MusicEnabled";
    private const string EffectsEnabledKey = "Audio_EffectsEnabled";
    private const string MusicVolumeKey = "Audio_MusicVolume";
    private const string EffectsVolumeKey = "Audio_EffectsVolume";

    [Header("Sources")]
    [SerializeField] private List<AudioSource> _musicSources = new();
    [SerializeField] private List<AudioSource> _effectsSources = new();

    [Header("Defaults")]
    [Range(0f, 1f)] [SerializeField] private float _defaultMusicVolume = 0.5f;
    [Range(0f, 1f)] [SerializeField] private float _defaultEffectsVolume = 0.5f;

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
        IsMusicEnabled = PlayerPrefs.GetInt(MusicEnabledKey, 1) == 1;
        AreEffectsEnabled = PlayerPrefs.GetInt(EffectsEnabledKey, 1) == 1;
        MusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, _defaultMusicVolume);
        EffectsVolume = PlayerPrefs.GetFloat(EffectsVolumeKey, _defaultEffectsVolume);
    }

    private void Save()
    {
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
        source.volume = MusicVolume;
    }

    private void ApplyEffectsSource(AudioSource source)
    {
        if (source == null)
            return;

        source.mute = !AreEffectsEnabled;
        source.volume = EffectsVolume;
    }
}
