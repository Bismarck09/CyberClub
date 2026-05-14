using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VisitorSpawner : MonoBehaviour
{
    [SerializeField] private DeviceRegistry _deviceRegistry;
    [SerializeField] private Transform _spawnPoint;

    [SerializeField] private List<GameObject> _visitorPrefabs;
    [SerializeField] private VisitorQueue _visitorQueue;

    [SerializeField] private float _baseSpawnDelay;         
    [SerializeField] private float _groupSpawnDelay;       
    [SerializeField] private int _minGroupSize;
    [SerializeField] private int _maxGroupSize;

    private int _currentVisitors;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            int maxVisitors = _deviceRegistry.CurrentDeviceCount;

            if (_currentVisitors >= maxVisitors || maxVisitors == 0)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            int freeSlots = maxVisitors - _currentVisitors;
            int groupSize = Random.Range(_minGroupSize, _maxGroupSize + 1);
            groupSize = Mathf.Min(groupSize, freeSlots);

            yield return StartCoroutine(SpawnGroup(groupSize));

            float dynamicDelay = Mathf.Clamp(_baseSpawnDelay - maxVisitors * 0.2f, 1.5f, _baseSpawnDelay);
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

        var prefab = _visitorPrefabs[Random.Range(0, _visitorPrefabs.Count)];

        GameObject obj = Instantiate(prefab, _spawnPoint.position, Quaternion.identity);

        var movement = obj.GetComponent<VisitorMovement>();
        var registration = obj.GetComponent<VisitorRegistration>();

        registration.Init(movement, _visitorQueue);

        _currentVisitors++;

        var visitorExit = obj.GetComponent<VisitorExit>();
        visitorExit.OnVisitorExit += OnVisitorLeft;
    }

    private void OnVisitorLeft()
    {
        _currentVisitors--;
    }
}
