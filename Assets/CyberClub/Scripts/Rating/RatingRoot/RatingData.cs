using System;
using UnityEngine;

public class RatingData : MonoBehaviour
{
    [Header("Rating")]
    [SerializeField] private float _startRating = 3f;
    [SerializeField] private float _minRating = 1f;
    [SerializeField] private float _maxRating = 5f;

    [Header("Income multiplier")]
    [SerializeField] private float _minIncomeMultiplier = 1f;
    [SerializeField] private float _maxIncomeMultiplier = 2f;

    [Header("Visitor spawn multiplier")]
    [SerializeField] private float _minVisitorCapacityMultiplier = 0.6f;
    [SerializeField] private float _maxVisitorCapacityMultiplier = 1.5f;

    [Header("Spawn delay multiplier")]
    [SerializeField] private float _maxSpawnDelayMultiplier = 1.5f;
    [SerializeField] private float _minSpawnDelayMultiplier = 0.7f;

    private float _currentRating;
    private bool _isRatingDropProtected;

    public float CurrentRating => _currentRating;
    public float MinRating => _minRating;
    public float MaxRating => _maxRating;
    public bool IsRatingDropProtected => _isRatingDropProtected;

    public float NormalizedRating => Mathf.InverseLerp(_minRating, _maxRating, _currentRating);
    public float IncomeMultiplier => Mathf.Lerp(_minIncomeMultiplier, _maxIncomeMultiplier, NormalizedRating);
    public float VisitorCapacityMultiplier => Mathf.Lerp(_minVisitorCapacityMultiplier, _maxVisitorCapacityMultiplier, NormalizedRating);
    public float SpawnDelayMultiplier => Mathf.Lerp(_maxSpawnDelayMultiplier, _minSpawnDelayMultiplier, NormalizedRating);

    public event Action<float> OnRatingChanged;

    private void Awake()
    {
        _currentRating = Mathf.Clamp(_startRating, _minRating, _maxRating);
    }

    private void Start()
    {
        OnRatingChanged?.Invoke(0f);
    }

    public void AddRating(float amount) => ChangeRating(Mathf.Abs(amount));
    public void RemoveRating(float amount) => ChangeRating(-Mathf.Abs(amount));

    public void ChangeRating(float amount)
    {
        if (amount < 0f && _isRatingDropProtected)
            return;

        float previous = _currentRating;
        _currentRating = Mathf.Clamp(_currentRating + amount, _minRating, _maxRating);

        float realAmount = _currentRating - previous;

        if (Mathf.Approximately(realAmount, 0f))
            return;

        OnRatingChanged?.Invoke(realAmount);
    }

    public void SetRating(float value)
    {
        float previous = _currentRating;
        _currentRating = Mathf.Clamp(value, _minRating, _maxRating);

        float realAmount = _currentRating - previous;
        OnRatingChanged?.Invoke(realAmount);
    }

    public void SetRatingDropProtection(bool value)
    {
        _isRatingDropProtected = value;
    }
}
