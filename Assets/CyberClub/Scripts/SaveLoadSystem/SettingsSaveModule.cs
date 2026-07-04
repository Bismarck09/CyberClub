using UnityEngine;

public class SettingsSaveModule : MonoBehaviour, ISaveModule
{
    [SerializeField] private AudioSettingsService _audioSettingsService;

    public void Capture(GameSaveData saveData)
    {
        if (_audioSettingsService == null)
            return;

        saveData.Settings.HasSettings = true;
        saveData.Settings.MusicEnabled = _audioSettingsService.IsMusicEnabled;
        saveData.Settings.EffectsEnabled = _audioSettingsService.AreEffectsEnabled;
        saveData.Settings.MusicVolume = _audioSettingsService.MusicVolume;
        saveData.Settings.EffectsVolume = _audioSettingsService.EffectsVolume;
    }

    public void Restore(GameSaveData saveData)
    {
        if (_audioSettingsService == null || saveData.Settings.HasSettings == false)
            return;

        _audioSettingsService.SetMusicEnabled(saveData.Settings.MusicEnabled);
        _audioSettingsService.SetEffectsEnabled(saveData.Settings.EffectsEnabled);
        _audioSettingsService.SetMusicVolume(saveData.Settings.MusicVolume);
        _audioSettingsService.SetEffectsVolume(saveData.Settings.EffectsVolume);
    }
}