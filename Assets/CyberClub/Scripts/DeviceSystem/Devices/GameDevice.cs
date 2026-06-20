using System.Collections;
using UnityEngine;

public class GameDevice : MonoBehaviour
{
    [SerializeField] private Transform _targetPoint;
    [SerializeField] private Transform _sitPoint;

    private bool _isOccupied;
    private Coroutine _sessionCoroutine;

    public bool IsOccupied => _isOccupied;
    public Transform TargetPoint => _targetPoint;
    public Transform SitPoint => _sitPoint;

    public bool TryReserve()
    {
        if (_isOccupied)
            return false;

        _isOccupied = true;
        return true;
    }

    public void Release()
    {
        if (_sessionCoroutine != null)
        {
            StopCoroutine(_sessionCoroutine);
            _sessionCoroutine = null;
        }

        _isOccupied = false;
    }
    
    public void Reserve(float time, VisitorExit visitorExit)
    {
        if (!_isOccupied)
            _isOccupied = true;

        StartReleaseCoroutine(time, visitorExit, null);
    }

    public void StartSession(float time, VisitorExit visitorExit, VisitorSeat seatController)
    {
        if (!_isOccupied)
            _isOccupied = true;

        StartReleaseCoroutine(time, visitorExit, seatController);
    }

    private void StartReleaseCoroutine(float time, VisitorExit visitorExit, VisitorSeat seatController)
    {
        if (_sessionCoroutine != null)
            StopCoroutine(_sessionCoroutine);

        _sessionCoroutine = StartCoroutine(ReleaseAfterTime(time, visitorExit, seatController));
    }

    private IEnumerator ReleaseAfterTime(float time, VisitorExit visitorExit, VisitorSeat seatController)
    {
        yield return new WaitForSeconds(time);

        if (seatController != null)
            seatController.StandUp(TargetPoint);

        _isOccupied = false;
        _sessionCoroutine = null;

        if (visitorExit != null)
            visitorExit.MoveToExit();
    }
}
