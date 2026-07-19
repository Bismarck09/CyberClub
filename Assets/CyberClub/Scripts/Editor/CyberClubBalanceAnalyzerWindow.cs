using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CyberClubBalanceAnalyzerWindow : EditorWindow
{
    private readonly List<ZoneDeviceConfig> _configs = new();

    private int _selectedConfig;
    private int _deviceCount = 1;
    private int _adminCount = 1;
    private float _adminServiceSeconds = 2f;
    private float _sessionSeconds = 8f;
    private float _travelSeconds = 2f;
    private float _rating = 3f;
    private float _roomBonus;
    private float _baseSpawnDelay = 3f;
    private float _spawnDelayPerDevice = 0.2f;
    private float _minimumSpawnDelay = 1.5f;
    private float _averageGroupSize = 5.5f;
    private float _groupSpawnDelay = 0.5f;
    private float _minimumRating = 1f;
    private float _maximumRating = 5f;
    private float _minimumIncomeMultiplier = 1f;
    private float _maximumIncomeMultiplier = 2f;
    private float _minimumVisitorCapacityMultiplier = 0.75f;
    private float _maximumVisitorCapacityMultiplier = 1.35f;
    private float _lowRatingSpawnDelayMultiplier = 1.25f;
    private float _highRatingSpawnDelayMultiplier = 0.75f;
    private int _currentCoins;
    private int _adminUpgradeTargetPrice;
    private int _interiorTargetPrice;
    private int _locationTargetPrice;
    private Vector2 _scroll;

    [MenuItem("CyberClub/Balance/Progression Analyzer")]
    public static void Open()
    {
        GetWindow<CyberClubBalanceAnalyzerWindow>("CyberClub Balance");
    }

    private void OnEnable()
    {
        ReloadConfigs();
    }

    private void OnGUI()
    {
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        EditorGUILayout.LabelField("Воспроизводимая модель экономики", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Модель повторяет формулы RatingData, VisitorSpawner, VisitorService и ResourcesWallet. " +
            "Время пути задаётся явно, потому что оно зависит от NavMesh и геометрии сцены.",
            MessageType.Info);

        if (_configs.Count == 0)
        {
            EditorGUILayout.HelpBox("ZoneDeviceConfig не найдены.", MessageType.Warning);
            if (GUILayout.Button("Обновить"))
                ReloadConfigs();
            EditorGUILayout.EndScrollView();
            return;
        }

        string[] names = _configs.Select(config => config.name).ToArray();
        _selectedConfig = EditorGUILayout.Popup("Конфигурация зоны", _selectedConfig, names);
        _deviceCount = EditorGUILayout.IntSlider("Компьютеров", _deviceCount, 0, 30);
        _adminCount = EditorGUILayout.IntSlider("Администраторов", _adminCount, 0, 3);
        _adminServiceSeconds = EditorGUILayout.Slider("Обслуживание админа, сек", _adminServiceSeconds, 0.3f, 5f);
        _sessionSeconds = EditorGUILayout.Slider("Сессия, сек", _sessionSeconds, 1f, 30f);
        _travelSeconds = EditorGUILayout.Slider("Средний путь, сек", _travelSeconds, 0f, 20f);
        _rating = EditorGUILayout.Slider("Рейтинг", _rating, 1f, 5f);
        _roomBonus = EditorGUILayout.Slider("Бонус интерьера", _roomBonus, 0f, 5f);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Параметры MainScene", EditorStyles.boldLabel);
        _baseSpawnDelay = EditorGUILayout.FloatField("Базовая задержка спавна", _baseSpawnDelay);
        _spawnDelayPerDevice = EditorGUILayout.FloatField("Снижение на устройство", _spawnDelayPerDevice);
        _minimumSpawnDelay = EditorGUILayout.FloatField("Минимальная задержка", _minimumSpawnDelay);
        _averageGroupSize = EditorGUILayout.FloatField("Средний размер группы", _averageGroupSize);
        _groupSpawnDelay = EditorGUILayout.FloatField("Интервал внутри группы", _groupSpawnDelay);
        _minimumRating = EditorGUILayout.FloatField("Минимальный рейтинг", _minimumRating);
        _maximumRating = EditorGUILayout.FloatField("Максимальный рейтинг", _maximumRating);
        _minimumIncomeMultiplier = EditorGUILayout.FloatField("Доход при min рейтинге", _minimumIncomeMultiplier);
        _maximumIncomeMultiplier = EditorGUILayout.FloatField("Доход при max рейтинге", _maximumIncomeMultiplier);
        _minimumVisitorCapacityMultiplier = EditorGUILayout.FloatField("Посетители при min рейтинге", _minimumVisitorCapacityMultiplier);
        _maximumVisitorCapacityMultiplier = EditorGUILayout.FloatField("Посетители при max рейтинге", _maximumVisitorCapacityMultiplier);
        _lowRatingSpawnDelayMultiplier = EditorGUILayout.FloatField("Задержка при min рейтинге", _lowRatingSpawnDelayMultiplier);
        _highRatingSpawnDelayMultiplier = EditorGUILayout.FloatField("Задержка при max рейтинге", _highRatingSpawnDelayMultiplier);

        _currentCoins = Mathf.Max(0, EditorGUILayout.IntField("Текущие монеты", _currentCoins));
        _adminUpgradeTargetPrice = Mathf.Max(0, EditorGUILayout.IntField("Цена апгрейда админа", _adminUpgradeTargetPrice));
        _interiorTargetPrice = Mathf.Max(0, EditorGUILayout.IntField("Цена интерьера", _interiorTargetPrice));
        _locationTargetPrice = Mathf.Max(0, EditorGUILayout.IntField("Цена следующей зоны", _locationTargetPrice));

        ZoneDeviceConfig config = _configs[Mathf.Clamp(_selectedConfig, 0, _configs.Count - 1)];
        BalanceScenario baseScenario = CreateScenario(config, 1f);
        BalanceEstimate baseEstimate = CyberClubBalanceModel.Calculate(baseScenario);
        BalanceEstimate speedEstimate = CyberClubBalanceModel.Calculate(CreateScenario(config, 2f));

        EditorGUILayout.Space();
        DrawEstimate("Без зелий", config, baseEstimate);
        DrawEstimate("Зелье скорости x2", config, speedEstimate);

        if (GUILayout.Button("Вывести отчёт всех ZoneDeviceConfig в Console"))
            PrintAllConfigs();

        EditorGUILayout.EndScrollView();
    }

    private BalanceScenario CreateScenario(ZoneDeviceConfig config, float speedMultiplier)
    {
        return new BalanceScenario
        {
            BaseIncomePerSession = config.PriceOfHourCoins,
            DeviceCount = _deviceCount,
            SessionSeconds = _sessionSeconds,
            AverageTravelSeconds = _travelSeconds,
            AdminCount = _adminCount,
            AdminServiceSeconds = _adminServiceSeconds,
            Rating = _rating,
            RoomBonus = _roomBonus,
            SpeedMultiplier = speedMultiplier,
            BaseSpawnDelay = Mathf.Max(0.05f, _baseSpawnDelay),
            SpawnDelayPerDevice = Mathf.Max(0f, _spawnDelayPerDevice),
            MinimumSpawnDelay = Mathf.Max(0.05f, _minimumSpawnDelay),
            AverageGroupSize = Mathf.Max(1f, _averageGroupSize),
            GroupSpawnDelay = Mathf.Max(0f, _groupSpawnDelay),
            MinimumRating = _minimumRating,
            MaximumRating = Mathf.Max(_minimumRating + 0.01f, _maximumRating),
            MinimumIncomeMultiplier = Mathf.Max(0f, _minimumIncomeMultiplier),
            MaximumIncomeMultiplier = Mathf.Max(0f, _maximumIncomeMultiplier),
            MinimumVisitorCapacityMultiplier = Mathf.Max(0f, _minimumVisitorCapacityMultiplier),
            MaximumVisitorCapacityMultiplier = Mathf.Max(0f, _maximumVisitorCapacityMultiplier),
            LowRatingSpawnDelayMultiplier = Mathf.Max(0f, _lowRatingSpawnDelayMultiplier),
            HighRatingSpawnDelayMultiplier = Mathf.Max(0f, _highRatingSpawnDelayMultiplier)
        };
    }

    private void DrawEstimate(string label, ZoneDeviceConfig config, BalanceEstimate estimate)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Доход/мин", estimate.CoinsPerMinute.ToString("N0"));
        EditorGUILayout.LabelField("Сессий/мин", estimate.SessionsPerMinute.ToString("0.00"));
        EditorGUILayout.LabelField("Ограничение", estimate.Bottleneck);
        EditorGUILayout.LabelField(
            "Активные посетители / предел",
            $"{estimate.VisitorCapacity:0} / {estimate.VisitorThroughput:0.00} сесс./мин");

        int nextDevicePrice = config.CalculateDevicePrice(_deviceCount);
        EditorGUILayout.LabelField("Следующий компьютер", FormatWait(_currentCoins, nextDevicePrice, estimate.CoinsPerMinute));

        if (_adminUpgradeTargetPrice > 0)
            EditorGUILayout.LabelField("Апгрейд администратора", FormatWait(_currentCoins, _adminUpgradeTargetPrice, estimate.CoinsPerMinute));

        if (_interiorTargetPrice > 0)
            EditorGUILayout.LabelField("Интерьер", FormatWait(_currentCoins, _interiorTargetPrice, estimate.CoinsPerMinute));

        if (_locationTargetPrice > 0)
            EditorGUILayout.LabelField("Следующая зона", FormatWait(_currentCoins, _locationTargetPrice, estimate.CoinsPerMinute));

        if (_deviceCount == 0 && _currentCoins < nextDevicePrice)
            EditorGUILayout.HelpBox("Softlock: нет дохода и не хватает монет на первый компьютер.", MessageType.Error);

        EditorGUILayout.Space();
    }

    private static string FormatWait(int currentCoins, int price, float coinsPerMinute)
    {
        float seconds = CyberClubBalanceModel.SecondsToAfford(currentCoins, price, coinsPerMinute);
        return float.IsPositiveInfinity(seconds)
            ? $"{price:N0}: недостижимо без внешней награды"
            : $"{price:N0}: {seconds / 60f:0.00} мин";
    }

    private void ReloadConfigs()
    {
        _configs.Clear();

        foreach (string guid in AssetDatabase.FindAssets("t:ZoneDeviceConfig"))
        {
            ZoneDeviceConfig config = AssetDatabase.LoadAssetAtPath<ZoneDeviceConfig>(AssetDatabase.GUIDToAssetPath(guid));

            if (config != null)
                _configs.Add(config);
        }

        _configs.Sort((left, right) => left.DevicePrice.CompareTo(right.DevicePrice));
        _selectedConfig = Mathf.Clamp(_selectedConfig, 0, Mathf.Max(0, _configs.Count - 1));
        Repaint();
    }

    private void PrintAllConfigs()
    {
        foreach (ZoneDeviceConfig config in _configs)
        {
            BalanceEstimate estimate = CyberClubBalanceModel.Calculate(CreateScenario(config, 1f));
            BalanceEstimate speed = CyberClubBalanceModel.Calculate(CreateScenario(config, 2f));
            Debug.Log(
                $"[Balance] {config.name}: income/session={config.PriceOfHourCoins:N0}, " +
                $"device={config.DevicePrice:N0}, growth={config.PriceGrowthPercent:0.##}%, " +
                $"base={estimate.CoinsPerMinute:N0}/min ({estimate.Bottleneck}), " +
                $"speed-x2={speed.CoinsPerMinute:N0}/min ({speed.Bottleneck}).");
        }
    }
}
