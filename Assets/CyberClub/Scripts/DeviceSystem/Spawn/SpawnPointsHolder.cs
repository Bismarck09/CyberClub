using UnityEngine;
using System.Collections.Generic;

public class SpawnPointsHolder : MonoBehaviour
{
    [SerializeField] private List<Transform> _spawnPoints;

      
    public Transform GetSpawnPoint()
    {
        if (_spawnPoints.Count == 0)
            return null;

        Transform spawnPoint = _spawnPoints[0];
        _spawnPoints.RemoveAt(0);
        return spawnPoint;
    }
}
