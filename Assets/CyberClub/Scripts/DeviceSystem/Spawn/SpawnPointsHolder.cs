using UnityEngine;
using System.Collections.Generic;

public class SpawnPointsHolder : MonoBehaviour
{
    [SerializeField] private List<Transform> _spawnPoints;

      
    public Transform GetSpawnPoint()
    {
        Transform spawnPoint = _spawnPoints[0];
        _spawnPoints.RemoveAt(0);
        return spawnPoint;
    }
}
