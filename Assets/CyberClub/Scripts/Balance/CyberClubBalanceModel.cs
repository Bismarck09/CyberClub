using System;
using UnityEngine;

[Serializable]
public struct BalanceScenario
{
    public int BaseIncomePerSession;
    public int DeviceCount;
    public float SessionSeconds;
    public float AverageTravelSeconds;
    public int AdminCount;
    public float AdminServiceSeconds;
    public float Rating;
    public float RoomBonus;
    public float SpeedMultiplier;
    public float BaseSpawnDelay;
    public float SpawnDelayPerDevice;
    public float MinimumSpawnDelay;
    public float AverageGroupSize;
    public float GroupSpawnDelay;
    public float MinimumRating;
    public float MaximumRating;
    public float MinimumIncomeMultiplier;
    public float MaximumIncomeMultiplier;
    public float MinimumVisitorCapacityMultiplier;
    public float MaximumVisitorCapacityMultiplier;
    public float LowRatingSpawnDelayMultiplier;
    public float HighRatingSpawnDelayMultiplier;
}

public readonly struct BalanceEstimate
{
    public BalanceEstimate(
        float sessionsPerMinute,
        float coinsPerMinute,
        float deviceCapacity,
        float adminCapacity,
        float spawnCapacity,
        float visitorCapacity,
        float visitorThroughput,
        string bottleneck)
    {
        SessionsPerMinute = sessionsPerMinute;
        CoinsPerMinute = coinsPerMinute;
        DeviceCapacity = deviceCapacity;
        AdminCapacity = adminCapacity;
        SpawnCapacity = spawnCapacity;
        VisitorCapacity = visitorCapacity;
        VisitorThroughput = visitorThroughput;
        Bottleneck = bottleneck;
    }

    public float SessionsPerMinute { get; }
    public float CoinsPerMinute { get; }
    public float DeviceCapacity { get; }
    public float AdminCapacity { get; }
    public float SpawnCapacity { get; }
    public float VisitorCapacity { get; }
    public float VisitorThroughput { get; }
    public string Bottleneck { get; }
}

public static class CyberClubBalanceModel
{
    public static BalanceEstimate Calculate(BalanceScenario scenario)
    {
        int devices = Mathf.Max(0, scenario.DeviceCount);
        int admins = Mathf.Max(0, scenario.AdminCount);
        float speed = Mathf.Max(1f, scenario.SpeedMultiplier);
        float session = Mathf.Max(0.1f, scenario.SessionSeconds) / speed;
        float travel = Mathf.Max(0f, scenario.AverageTravelSeconds) / speed;
        float service = Mathf.Max(0.05f, scenario.AdminServiceSeconds) / speed;

        float minimumRating = scenario.MaximumRating > scenario.MinimumRating
            ? scenario.MinimumRating
            : 1f;
        float maximumRating = scenario.MaximumRating > scenario.MinimumRating
            ? scenario.MaximumRating
            : 5f;
        float ratingNormalized = Mathf.InverseLerp(
            minimumRating,
            maximumRating,
            Mathf.Clamp(scenario.Rating, minimumRating, maximumRating));
        float ratingIncomeMultiplier = Mathf.Lerp(
            Mathf.Max(0f, scenario.MinimumIncomeMultiplier),
            Mathf.Max(0f, scenario.MaximumIncomeMultiplier),
            ratingNormalized);
        float visitorCapacityMultiplier = Mathf.Lerp(
            Mathf.Max(0f, scenario.MinimumVisitorCapacityMultiplier),
            Mathf.Max(0f, scenario.MaximumVisitorCapacityMultiplier),
            ratingNormalized);
        float spawnDelayMultiplier = Mathf.Lerp(
            Mathf.Max(0f, scenario.LowRatingSpawnDelayMultiplier),
            Mathf.Max(0f, scenario.HighRatingSpawnDelayMultiplier),
            ratingNormalized);

        float deviceCapacity = devices > 0 ? devices * 60f / (session + travel) : 0f;
        float adminCapacity = admins > 0 ? admins * 60f / service : 0f;
        float baseSpawnDelay = Mathf.Max(0.05f, scenario.BaseSpawnDelay);
        float minimumSpawnDelay = Mathf.Clamp(
            scenario.MinimumSpawnDelay,
            0.05f,
            baseSpawnDelay);
        float dynamicSpawnDelay = Mathf.Clamp(
            baseSpawnDelay - devices * Mathf.Max(0f, scenario.SpawnDelayPerDevice),
            minimumSpawnDelay,
            baseSpawnDelay);
        float averageGroupSize = Mathf.Max(1f, scenario.AverageGroupSize);
        float averageGroupSpanSeconds = averageGroupSize * Mathf.Max(0f, scenario.GroupSpawnDelay);
        float spawnCapacity = devices > 0
            ? averageGroupSize * 60f /
              (dynamicSpawnDelay * spawnDelayMultiplier + averageGroupSpanSeconds)
            : 0f;
        float visitorCapacity = devices > 0
            ? Mathf.Max(1, Mathf.RoundToInt(devices * visitorCapacityMultiplier))
            : 0f;
        float visitorLifecycle = service + session + travel * 2f;
        float visitorThroughput = visitorCapacity > 0f
            ? visitorCapacity * 60f / Mathf.Max(0.1f, visitorLifecycle)
            : 0f;

        float sessionsPerMinute = Mathf.Min(
            deviceCapacity,
            adminCapacity,
            spawnCapacity,
            visitorThroughput);
        float incomePerSession = Mathf.RoundToInt(
            Mathf.Max(0, scenario.BaseIncomePerSession) *
            Mathf.Max(0f, 1f + scenario.RoomBonus) *
            ratingIncomeMultiplier);
        float coinsPerMinute = sessionsPerMinute * incomePerSession;

        string bottleneck = deviceCapacity <= adminCapacity &&
            deviceCapacity <= spawnCapacity &&
            deviceCapacity <= visitorThroughput
            ? "устройства"
            : adminCapacity <= spawnCapacity && adminCapacity <= visitorThroughput
                ? "администраторы"
                : spawnCapacity <= visitorThroughput
                    ? "появление посетителей"
                    : "лимит активных посетителей";

        return new BalanceEstimate(
            sessionsPerMinute,
            coinsPerMinute,
            deviceCapacity,
            adminCapacity,
            spawnCapacity,
            visitorCapacity,
            visitorThroughput,
            bottleneck);
    }

    public static float SecondsToAfford(int currentBalance, int targetPrice, float coinsPerMinute)
    {
        int missing = Mathf.Max(0, targetPrice - currentBalance);

        if (missing == 0)
            return 0f;

        return coinsPerMinute > 0.0001f
            ? missing / coinsPerMinute * 60f
            : float.PositiveInfinity;
    }
}
