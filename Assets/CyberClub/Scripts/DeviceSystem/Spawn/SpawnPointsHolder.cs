using UnityEngine;
using System.Collections.Generic;

public class SpawnPointsHolder : MonoBehaviour
{
    [SerializeField] private List<Transform> _spawnPoints;

    private readonly List<Transform> _runtimeSpawnPoints = new();

    public bool HasSpawnPoints => _runtimeSpawnPoints.Exists(point => point != null);
    public int AvailableSpawnPointCount => _runtimeSpawnPoints.FindAll(point => point != null).Count;

    private void Awake()
    {
        ResetSpawnPoints();
    }

    public Transform GetSpawnPoint()
    {
        RemoveInvalidRuntimePoints();

        if (_runtimeSpawnPoints.Count == 0)
            return null;

        Transform spawnPoint = _runtimeSpawnPoints[0];
        _runtimeSpawnPoints.RemoveAt(0);
        return spawnPoint;
    }

    public void ReleaseSpawnPoint(Transform spawnPoint)
    {
        if (spawnPoint == null || _runtimeSpawnPoints.Contains(spawnPoint))
            return;

        int originalIndex = _spawnPoints != null ? _spawnPoints.IndexOf(spawnPoint) : -1;

        if (originalIndex < 0)
            return;

        int insertIndex = _runtimeSpawnPoints.FindIndex(point =>
            _spawnPoints.IndexOf(point) > originalIndex);

        if (insertIndex < 0)
            _runtimeSpawnPoints.Add(spawnPoint);
        else
            _runtimeSpawnPoints.Insert(insertIndex, spawnPoint);
    }

    public void ResetSpawnPoints()
    {
        _runtimeSpawnPoints.Clear();

        if (_spawnPoints == null)
            return;

        foreach (Transform spawnPoint in _spawnPoints)
        {
            if (spawnPoint != null && !_runtimeSpawnPoints.Contains(spawnPoint))
                _runtimeSpawnPoints.Add(spawnPoint);
        }
    }

    private void RemoveInvalidRuntimePoints()
    {
        _runtimeSpawnPoints.RemoveAll(point => point == null);
    }
}
