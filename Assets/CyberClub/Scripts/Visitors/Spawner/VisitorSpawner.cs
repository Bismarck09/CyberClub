using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
            int deviceCount = _deviceRegistry.CurrentDeviceCount;
            int maxVisitors = GetMaxVisitors(deviceCount);

            if (_currentVisitors >= maxVisitors || maxVisitors == 0)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            int freeSlots = maxVisitors - _currentVisitors;
            int groupSize = Random.Range(_minGroupSize, _maxGroupSize + 1);
            groupSize = Mathf.Min(groupSize, freeSlots);

            yield return StartCoroutine(SpawnGroup(groupSize));

            float dynamicDelay = Mathf.Clamp(_baseSpawnDelay - deviceCount * 0.2f, 1.5f, _baseSpawnDelay);
            dynamicDelay *= GetRatingSpawnDelayMultiplier();

            yield return new WaitForSeconds(dynamicDelay);
        }
    }

    private IEnumerator SpawnGroup(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnOne();
            yield return new WaitForSeconds(_groupSpawnDelay);
        }
    }

    private void SpawnOne()
    {
        if (_visitorPrefabs.Count == 0 || _spawnPoint == null)
            return;

        GameObject prefab = _visitorPrefabs[Random.Range(0, _visitorPrefabs.Count)];
        GameObject obj = Instantiate(prefab, _spawnPoint.position, Quaternion.identity);

        if (obj.GetComponent<VisitorRatingTracker>() == null)
            obj.AddComponent<VisitorRatingTracker>();

        VisitorMovement movement = obj.GetComponent<VisitorMovement>();
        VisitorRegistration registration = obj.GetComponent<VisitorRegistration>();

        if (registration != null)
            registration.Init(movement, _visitorQueue);

        _currentVisitors++;

        VisitorExit visitorExit = obj.GetComponent<VisitorExit>();

        if (visitorExit != null)
            visitorExit.OnVisitorExit += OnVisitorLeft;
    }

    private void OnVisitorLeft()
    {
        _currentVisitors = Mathf.Max(0, _currentVisitors - 1);
    }

    private int GetMaxVisitors(int deviceCount)
    {
        if (deviceCount <= 0)
            return 0;

        float multiplier = _ratingData != null ? _ratingData.VisitorCapacityMultiplier : 1f;
        return Mathf.Max(1, Mathf.RoundToInt(deviceCount * multiplier));
    }

    private float GetRatingSpawnDelayMultiplier()
    {
        return _ratingData != null ? _ratingData.SpawnDelayMultiplier : 1f;
    }
}
