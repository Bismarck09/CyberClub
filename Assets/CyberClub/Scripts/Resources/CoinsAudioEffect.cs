using UnityEngine;

public class CoinsAudioEffect : MonoBehaviour
{
    [SerializeField] private AudioSource _addCoinsSource;
    [SerializeField] private AudioSource _removeCoinsSource;
    [SerializeField] private CoinsData _coinsData;


    private void OnEnable()
    {
        _coinsData.OnCoinsChanged += PlaySound;
    }

    private void OnDisable()
    {
        _coinsData.OnCoinsChanged -= PlaySound;
    }

    private void PlaySound(int amount)
    {
        if (amount > 0)
            _addCoinsSource.Play();
        else 
            _removeCoinsSource.Play();
    }
}
