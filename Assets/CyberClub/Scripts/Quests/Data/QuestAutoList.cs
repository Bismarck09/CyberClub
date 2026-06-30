using System;
using System.Collections.Generic;
using UnityEngine;

public static class QuestAutoList
{
    private const int DefaultQuestCount = 100;
    private const int DefaultStartVisitorTarget = 10;
    private const int DefaultVisitorTargetStep = 5;
    private const int DefaultMinDeviceTarget = 2;
    private const int DefaultMaxDeviceTarget = 4;
    private const int DefaultSeed = 12345;
    private const int DefaultStartReward = 1600;
    private const float DefaultRewardGrowth = 1.08f;
    private const int DefaultRewardFlatBonus = 350;

    public static List<QuestData> Create()
    {
        return Create(
            DefaultQuestCount,
            DefaultStartVisitorTarget,
            DefaultVisitorTargetStep,
            DefaultMinDeviceTarget,
            DefaultMaxDeviceTarget,
            DefaultSeed,
            DefaultStartReward,
            DefaultRewardGrowth,
            DefaultRewardFlatBonus
        );
    }

    public static List<QuestData> Create(QuestGenerationSettings settings)
    {
        if (settings == null)
            return Create();

        return Create(
            settings.QuestCount,
            settings.StartVisitorTarget,
            settings.VisitorTargetStep,
            settings.MinDeviceTarget,
            settings.MaxDeviceTarget,
            settings.RandomSeed,
            settings.StartReward,
            settings.RewardGrowth,
            settings.RewardFlatBonus
        );
    }

    private static List<QuestData> Create(
        int questCount,
        int startVisitorTarget,
        int visitorTargetStep,
        int minDeviceTarget,
        int maxDeviceTarget,
        int seed,
        int startReward,
        float rewardGrowth,
        int rewardFlatBonus)
    {
        List<QuestData> result = new();

        AddTutorialQuests(result);

        int visitorTarget = Mathf.Max(1, startVisitorTarget);
        int reward = Mathf.Max(0, startReward);

        System.Random random = new(seed);

        while (result.Count < questCount)
        {
            int generatedIndex = result.Count - 10;

            bool isDeviceQuest = generatedIndex % 3 == 0;

            if (isDeviceQuest)
            {
                int deviceTarget = random.Next(minDeviceTarget, maxDeviceTarget + 1);
                reward = GetNextReward(reward, rewardGrowth, rewardFlatBonus);

                AddQuest(
                    result,
                    QuestType.BuyDevice,
                    deviceTarget,
                    reward,
                    $"Купи {deviceTarget} {GetComputerWord(deviceTarget)}"
                );

                continue;
            }

            reward = GetNextReward(reward, rewardGrowth, rewardFlatBonus);

            AddQuest(
                result,
                QuestType.VisitorService,
                visitorTarget,
                reward,
                $"Обслужи {visitorTarget} клиентов"
            );

            visitorTarget += visitorTargetStep;
        }

        return result;
    }

    private static void AddTutorialQuests(List<QuestData> result)
    {
        AddQuest(result, QuestType.BuyDevice, 1, 50, "Купи первый компьютер");
        AddQuest(result, QuestType.VisitorService, 1, 75, "Обслужи первого клиента");
        AddQuest(result, QuestType.BuyDevice, 1, 100, "Купи ещё один компьютер");
        AddQuest(result, QuestType.VisitorService, 3, 150, "Обслужи 3 клиентов");
        AddQuest(result, QuestType.BuyDevice, 2, 220, "Расширь клуб: купи 2 компьютера");
        AddQuest(result, QuestType.VisitorService, 5, 300, "Обслужи 5 клиентов");
        AddQuest(result, QuestType.VisitorService, 8, 450, "Проверь поток клиентов: обслужи 8 клиентов");
        AddQuest(result, QuestType.BuyDevice, 2, 600, "Поставь ещё 2 компьютера");
        AddQuest(result, QuestType.VisitorService, 12, 850, "Обслужи 12 клиентов");
        AddQuest(result, QuestType.BuyDevice, 3, 1200, "Купи 3 компьютера");
    }

    private static void AddQuest(List<QuestData> result, QuestType type, int target, int reward, string description)
    {
        result.Add(QuestData.CreateRuntime(type, target, reward, description));
    }

    private static int GetNextReward(int previousReward, float growth, int flatBonus)
    {
        int reward = Mathf.RoundToInt(previousReward * growth + flatBonus);
        return RoundReward(reward);
    }

    private static int RoundReward(int reward)
    {
        if (reward < 1000)
            return Mathf.RoundToInt(reward / 10f) * 10;

        if (reward < 10000)
            return Mathf.RoundToInt(reward / 50f) * 50;

        if (reward < 100000)
            return Mathf.RoundToInt(reward / 500f) * 500;

        if (reward < 1000000)
            return Mathf.RoundToInt(reward / 2500f) * 2500;

        return Mathf.RoundToInt(reward / 10000f) * 10000;
    }

    private static string GetComputerWord(int count)
    {
        int lastDigit = count % 10;
        int lastTwoDigits = count % 100;

        if (lastTwoDigits >= 11 && lastTwoDigits <= 14)
            return "компьютеров";

        if (lastDigit == 1)
            return "компьютер";

        if (lastDigit >= 2 && lastDigit <= 4)
            return "компьютера";

        return "компьютеров";
    }
}
