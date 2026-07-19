using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisitorSpawner : MonoBehaviour
{
    [SerializeField] private DeviceRegistry _deviceRegistry;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private List<GameObject> _visitorPrefabs;
    [SerializeField] private VisitorQueue _visitorQueue;

    [Header("Spawn")]
    [SerializeField] private float _baseSpawnDelay = 8f;
    [SerializeField] private float _groupSpawnDelay = 0.5f;
    [SerializeField] private int _minGroupSize = 1;
    [SerializeField] private int _maxGroupSize = 3;

    [Header("Rating")]
    [SerializeField] private RatingData _ratingData;

    private int _currentVisitors;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (_deviceRegistry == null ||
                _visitorQueue == null)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            int deviceCount = _deviceRegistry.CurrentDeviceCount;
            int maxVisitors = GetMaxVisitors(deviceCount);

            // ИЗМЕНЕНО: учитываем не только компьютеры,
            // но и фактическую вместимость очередей.
            if (_currentVisitors >= maxVisitors ||
                maxVisitors == 0 ||
                !_visitorQueue.HasFreeSlot())
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            int visitorCapacity =
                maxVisitors - _currentVisitors;

            int queueCapacity =
                _visitorQueue.FreeSlotCount;

            int groupSize =
                Random.Range(
                    _minGroupSize,
                    _maxGroupSize + 1);

            groupSize = Mathf.Min(
                groupSize,
                visitorCapacity,
                queueCapacity);

            if (groupSize > 0)
                yield return StartCoroutine(SpawnGroup(groupSize));

            float dynamicDelay = Mathf.Clamp(
                _baseSpawnDelay - deviceCount * 0.2f,
                1.5f,
                _baseSpawnDelay);

            dynamicDelay *=
                GetRatingSpawnDelayMultiplier();

            yield return new WaitForSeconds(dynamicDelay);
        }
    }

    private IEnumerator SpawnGroup(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // ИЗМЕНЕНО: повторная проверка перед каждым
            // посетителем, потому что очередь могла измениться.
            if (_visitorQueue == null ||
                !_visitorQueue.HasFreeSlot())
            {
                yield break;
            }

            int maxVisitors =
                GetMaxVisitors(_deviceRegistry.CurrentDeviceCount);

            if (_currentVisitors >= maxVisitors)
                yield break;

            SpawnOne();

            yield return new WaitForSeconds(_groupSpawnDelay);
        }
    }

    private bool SpawnOne()
    {
        if (_visitorPrefabs == null ||
            _visitorPrefabs.Count == 0 ||
            _spawnPoint == null ||
            _visitorQueue == null)
        {
            return false;
        }

        GameObject prefab =
            _visitorPrefabs[
                Random.Range(0, _visitorPrefabs.Count)];

        if (prefab == null)
            return false;

        GameObject obj = Instantiate(
            prefab,
            _spawnPoint.position,
            Quaternion.identity);

        if (obj.GetComponent<VisitorRatingTracker>() == null)
            obj.AddComponent<VisitorRatingTracker>();

        if (obj.GetComponent<VisitorSpeedBoostAdapter>() == null)
            obj.AddComponent<VisitorSpeedBoostAdapter>();

        VisitorMovement movement =
            obj.GetComponent<VisitorMovement>();

        VisitorRegistration registration =
            obj.GetComponent<VisitorRegistration>();

        VisitorExit visitorExit =
            obj.GetComponent<VisitorExit>();

        // ИЗМЕНЕНО: такой объект нельзя учитывать как посетителя,
        // иначе счётчик никогда не уменьшится.
        if (movement == null ||
            registration == null ||
            visitorExit == null)
        {
            Debug.LogError(
                $"VisitorSpawner: префаб {prefab.name} настроен неправильно.");

            Destroy(obj);
            return false;
        }

        // ИЗМЕНЕНО: сначала успешная регистрация,
        // потом увеличение счётчика.
        if (!registration.Init(movement, _visitorQueue))
        {
            Destroy(obj);
            return false;
        }

        visitorExit.OnVisitorExit += OnVisitorLeft;
        _currentVisitors++;

        return true;
    }

    private void OnVisitorLeft()
    {
        _currentVisitors =
            Mathf.Max(0, _currentVisitors - 1);
    }

    private int GetMaxVisitors(int deviceCount)
    {
        if (deviceCount <= 0)
            return 0;

        float multiplier =
            _ratingData != null
                ? _ratingData.VisitorCapacityMultiplier
                : 1f;

        return Mathf.Max(
            1,
            Mathf.RoundToInt(deviceCount * multiplier));
    }

    private float GetRatingSpawnDelayMultiplier()
    {
        return _ratingData != null
            ? _ratingData.SpawnDelayMultiplier
            : 1f;
    }
}
