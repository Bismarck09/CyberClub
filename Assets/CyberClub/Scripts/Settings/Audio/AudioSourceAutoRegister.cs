using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSourceAutoRegister : MonoBehaviour
{
    [SerializeField] private AudioSourceType _type = AudioSourceType.Effects;
    [SerializeField] private AudioSettingsService _audioSettingsService;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (_audioSettingsService == null)
            _audioSettingsService = FindAnyObjectByType<AudioSettingsService>();

        if (_audioSettingsService == null)
            return;

        if (_type == AudioSourceType.Music)
            _audioSettingsService.RegisterMusicSource(_audioSource);
        else
            _audioSettingsService.RegisterEffectsSource(_audioSource);
    }
}

public enum AudioSourceType
{
    Music,
    Effects
}