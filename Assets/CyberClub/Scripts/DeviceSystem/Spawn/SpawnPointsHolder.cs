using UnityEngine;
using System.Collections.Generic;

public class SpawnPointsHolder : MonoBehaviour
{
    [SerializeField] private List<Transform> _spawnPoints;

    private readonly List<Transform> _runtimeSpawnPoints = new();

    public bool HasSpawnPoints => _runtimeSpawnPoints.Count > 0;

    private void Awake()
    {
        ResetSpawnPoints();
    }

    public Transform GetSpawnPoint()
    {
        if (_runtimeSpawnPoints.Count == 0)
            return null;

        Transform spawnPoint = _runtimeSpawnPoints[0];
        _runtimeSpawnPoints.RemoveAt(0);
        return spawnPoint;
    }

    public void ResetSpawnPoints()
    {
        _runtimeSpawnPoints.Clear();
        _runtimeSpawnPoints.AddRange(_spawnPoints);
    }
}