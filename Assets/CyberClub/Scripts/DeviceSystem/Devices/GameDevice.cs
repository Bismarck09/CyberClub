using UnityEngine;
using System.Collections;

public class GameDevice : MonoBehaviour
{
    [SerializeField] private Transform _targetPoint;
    [SerializeField] private Transform _sitPoint;

    private bool _isOccupied;

    public bool IsOccupied => _isOccupied;
    public Transform TargetPoint => _targetPoint;
    public Transform SitPoint => _sitPoint;

    public void Reserve(float time)
    {
        _isOccupied = true;
        StartCoroutine(ReleaseAfterTime(time));
    }

    private IEnumerator ReleaseAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        _isOccupied = false;
    }


}
