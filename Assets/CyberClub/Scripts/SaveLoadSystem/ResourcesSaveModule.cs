using UnityEngine;

public class ResourcesSaveModule : MonoBehaviour, ISaveModule
{
    [SerializeField] private CoinsData _coinsData;
    [SerializeField] private GemsData _gemsData;
    [SerializeField] private RatingData _ratingData;

    public void Capture(GameSaveData saveData)
    {
        if (_coinsData != null)
            saveData.Resources.Coins = _coinsData.CurrentCoins;

        if (_gemsData != null)
            saveData.Resources.Gems = _gemsData.CurrentGems;

        if (_ratingData != null)
            saveData.Rating.CurrentRating = _ratingData.CurrentRating;
    }

    public void Restore(GameSaveData saveData)
    {
        if (!GameSaveRepository.HasSave)
            return;

        if (_coinsData != null)
            _coinsData.SetCoins(saveData.Resources.Coins);

        if (_gemsData != null)
            _gemsData.SetGems(saveData.Resources.Gems);

        if (_ratingData != null)
            _ratingData.SetRating(saveData.Rating.CurrentRating);
    }
}