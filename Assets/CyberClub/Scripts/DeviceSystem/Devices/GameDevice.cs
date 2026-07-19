using System;
using System.Collections;
using UnityEngine;

public class GameDevice : MonoBehaviour
{
    [SerializeField] private Transform _targetPoint;
    [SerializeField] private Transform _sitPoint;

    [Header("Repair bonus")]
    [SerializeField] private float _repairIncomeMultiplier = 3f;

    private bool _isOccupied;
    private bool _isBroken;
    private bool _hasRepairIncomeBonus;
    private bool _isApplicationQuitting;
    private Coroutine _sessionCoroutine;

    public bool IsOccupied => _isOccupied;
    public bool IsBroken => _isBroken;
    public bool IsAvailable => !_isOccupied && !_isBroken;
    public Transform TargetPoint => _targetPoint;
    public Transform SitPoint => _sitPoint;

    public event Action<GameDevice> OnBroken;
    public event Action<GameDevice> OnRepaired;
    public event Action<GameDevice, bool> OnBreakdownStateChanged;

    public bool TryReserve()
    {
        if (_isOccupied || _isBroken)
            return false;

        _isOccupied = true;
        return true;
    }

    public void Release()
    {
        // ИЗМЕНЕНО: Release идемпотентен и не пытается останавливать coroutine
        // на неактивном/уничтожаемом MonoBehaviour.
        Coroutine sessionCoroutine = _sessionCoroutine;
        _sessionCoroutine = null;

        if (sessionCoroutine != null &&
            !_isApplicationQuitting &&
            isActiveAndEnabled &&
            gameObject.activeInHierarchy)
        {
            StopCoroutine(sessionCoroutine);
        }

        _isOccupied = false;
    }

    public void Reserve(float time, VisitorExit visitorExit)
    {
        if (_isBroken)
            return;

        if (!_isOccupied)
            _isOccupied = true;

        StartReleaseCoroutine(time, visitorExit, null);
    }

    public void StartSession(float time, VisitorExit visitorExit, VisitorSeat seatController)
    {
        if (_isBroken)
            return;

        if (!_isOccupied)
            _isOccupied = true;

        StartReleaseCoroutine(time, visitorExit, seatController);
    }

    public bool BreakDown()
    {
        if (_isBroken || _isOccupied)
            return false;

        _isBroken = true;
        _hasRepairIncomeBonus = false;

        OnBroken?.Invoke(this);
        OnBreakdownStateChanged?.Invoke(this, true);

        return true;
    }

    public void Repair()
    {
        if (!_isBroken)
            return;

        _isBroken = false;
        _hasRepairIncomeBonus = true;

        OnRepaired?.Invoke(this);
        OnBreakdownStateChanged?.Invoke(this, false);
    }

    public float ConsumeRepairIncomeMultiplier()
    {
        if (!_hasRepairIncomeBonus)
            return 1f;

        _hasRepairIncomeBonus = false;
        return Mathf.Max(1f, _repairIncomeMultiplier);
    }

    private void StartReleaseCoroutine(float time, VisitorExit visitorExit, VisitorSeat seatController)
    {
        if (_sessionCoroutine != null)
            StopCoroutine(_sessionCoroutine);

        _sessionCoroutine = StartCoroutine(ReleaseAfterTime(time, visitorExit, seatController));
    }

    private void OnApplicationQuit()
    {
        _isApplicationQuitting = true;
        _sessionCoroutine = null;
    }

    private IEnumerator ReleaseAfterTime(float time, VisitorExit visitorExit, VisitorSeat seatController)
    {
        yield return new WaitForSeconds(time);

        if (seatController != null)
            seatController.StandUp(TargetPoint);

        _isOccupied = false;
        _sessionCoroutine = null;

        if (visitorExit != null)
        {
            visitorExit.ClearReservedDevice(this);
            visitorExit.MoveToExit();
        }
    }
}
