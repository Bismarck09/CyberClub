using System;
using UnityEngine;

public class RatingData : MonoBehaviour
{
    private float _currentRating;
    public float CurrentRating => _currentRating;

    public event Action<float> OnRatingChanged;

    public void AddRating(float rating)
    {
        _currentRating += rating;
        _currentRating = Mathf.Clamp(_currentRating, 0f, 5f);
        OnRatingChanged?.Invoke(rating);
    }

    public void RemoveRating(float rating)
    {
        _currentRating += rating;
        _currentRating = Mathf.Clamp(_currentRating, 0f, 5f);
        OnRatingChanged?.Invoke(rating);
    }
}
