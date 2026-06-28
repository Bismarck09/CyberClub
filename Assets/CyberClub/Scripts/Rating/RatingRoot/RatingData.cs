using System;
using UnityEngine;

public class RatingData : MonoBehaviour
{
    [Header("Rating")]
    [SerializeField] private float _startRating = 3f;
    [SerializeField] private float _minRating = 1f;
    [SerializeField] private float _maxRating = 5f;

    [Header("Multiplier")]
    [SerializeField] private float _minIncomeMultiplier = 1f;
    [SerializeField] private float _maxIncomeMultiplier = 2f;

    [Header("Spawn influence")]
    [SerializeField] private float _minSpawnDelayMultiplier = 1.25f;
    [SerializeField] private float _maxSpawnDelayMultiplier = 0.75f;
    [SerializeField] private float _minVisitorCapacityMultiplier = 0.75f;
    [SerializeField] private float _maxVisitorCapacityMultiplier = 1.35f;

    private float _currentRating;
    private bool _isRatingDropProtected;

    public float CurrentRating => _currentRating;
    public float MinRating => _minRating;
    public float MaxRating => _maxRating;
    public bool IsRatingDropProtected => _isRatingDropProtected;

    public float NormalizedRating
    {
        get
        {
            if (Mathf.Approximately(_maxRating, _minRating))
                return 1f;

            return Mathf.InverseLerp(_minRating, _maxRating, _currentRating);
        }
    }

    public float IncomeMultiplier => Mathf.Lerp(_minIncomeMultiplier, _maxIncomeMultiplier, NormalizedRating);
    public float SpawnDelayMultiplier => Mathf.Lerp(_minSpawnDelayMultiplier, _maxSpawnDelayMultiplier, NormalizedRating);
    public float VisitorCapacityMultiplier => Mathf.Lerp(_minVisitorCapacityMultiplier, _maxVisitorCapacityMultiplier, NormalizedRating);

    public event Action<float> OnRatingChanged;
    public event Action<bool> OnRatingProtectionChanged;

    private void Awake()
    {
        _currentRating = Mathf.Clamp(_startRating, _minRating, _maxRating);
    }

    private void Start()
    {
        OnRatingChanged?.Invoke(0f);
    }

    public void AddRating(float amount)
    {
        ChangeRating(Mathf.Abs(amount));
    }

    public void RemoveRating(float amount)
    {
        ChangeRating(-Mathf.Abs(amount));
    }

    public void ChangeRating(float amount)
    {
        if (Mathf.Approximately(amount, 0f))
            return;

        if (amount < 0f && _isRatingDropProtected)
        {
            Debug.Log("Рейтинг не упал: активно зелье рейтинга.");
            OnRatingChanged?.Invoke(0f);
            return;
        }

        float previousRating = _currentRating;
        _currentRating = Mathf.Clamp(_currentRating + amount, _minRating, _maxRating);

        float realDelta = _currentRating - previousRating;

        if (Mathf.Approximately(realDelta, 0f))
            return;

        OnRatingChanged?.Invoke(realDelta);

        Debug.Log($"Рейтинг изменился: {previousRating:0.00} -> {_currentRating:0.00}. Множитель дохода: x{IncomeMultiplier:0.00}");
    }

    public void SetRatingDropProtection(bool value)
    {
        if (_isRatingDropProtected == value)
            return;

        _isRatingDropProtected = value;
        OnRatingProtectionChanged?.Invoke(_isRatingDropProtected);

        Debug.Log(value ? "Защита рейтинга включена." : "Защита рейтинга выключена.");
    }
}
