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

    public void Reserve(float time, VisitorExit visitorExit)
    {
        _isOccupied = true;
        StartCoroutine(ReleaseAfterTime(time, visitorExit));
    }

    private IEnumerator ReleaseAfterTime(float time, VisitorExit visitorExit)
    {
        yield return new WaitForSeconds(time);

        visitorExit.MoveToExit();
        _isOccupied = false;
    }


}
