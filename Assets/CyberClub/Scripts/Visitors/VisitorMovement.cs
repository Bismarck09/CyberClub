using System;
using UnityEngine;
using UnityEngine.AI;

public class VisitorMovement : MonoBehaviour
{
    private const string MovementParameter = "IsMovement";

    [Header("Fail-safe")]
    [SerializeField, Min(1f)]
    private float _moveTimeout = 30f;

    [SerializeField, Min(0.5f)]
    private float _stuckTimeout = 4f;

    [SerializeField, Min(0.01f)]
    private float _progressEpsilon = 0.05f;

    [SerializeField, Min(0.1f)]
    private float _navMeshSampleRadius = 1.5f;

    private Animator _animator;
    private NavMeshAgent _agent;

    private Action _onComplete;

    // ИЗМЕНЕНО: отдельный callback неудачного движения.
    private Action _onFailed;

    private Vector3 _targetPos;
    private bool _hasTarget;

    private float _moveStartedAt;
    private float _lastProgressAt;
    private float _lastRemainingDistance;

    public bool IsMoving { get; private set; }

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();

        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (!_hasTarget)
            return;

        CheckMovement();
    }

    // ИЗМЕНЕНО: метод возвращает, удалось ли вообще запустить движение.
    public bool Move(
        Vector3 target,
        Action onComplete = null,
        Action onFailed = null)
    {
        CancelCurrentMove();

        if (!TryPlaceAgentOnNavMesh())
        {
            Debug.LogWarning(
                $"VisitorMovement: {name} находится вне NavMesh.");

            return false;
        }

        if (!NavMesh.SamplePosition(
                target,
                out NavMeshHit targetHit,
                _navMeshSampleRadius,
                _agent.areaMask))
        {
            Debug.LogWarning(
                $"VisitorMovement: цель {target} находится вне NavMesh.");

            return false;
        }

        _targetPos = targetHit.position;
        _onComplete = onComplete;
        _onFailed = onFailed;

        _agent.updatePosition = true;
        _agent.updateRotation = true;
        _agent.isStopped = false;

        // ИЗМЕНЕНО: раньше результат SetDestination игнорировался.
        if (!_agent.SetDestination(_targetPos))
        {
            Debug.LogWarning(
                $"VisitorMovement: не удалось назначить путь для {name}.");

            CancelCurrentMove();
            return false;
        }

        _hasTarget = true;
        IsMoving = true;

        _moveStartedAt = Time.unscaledTime;
        _lastProgressAt = Time.unscaledTime;
        _lastRemainingDistance = Mathf.Infinity;

        SetMovementAnimation(true);
        return true;
    }

    public void StopMovement()
    {
        CancelCurrentMove();
    }

    private void CheckMovement()
    {
        if (_agent == null ||
            !_agent.enabled ||
            !_agent.isOnNavMesh)
        {
            FailCurrentMove("NavMeshAgent стал недоступен.");
            return;
        }

        if (Time.unscaledTime - _moveStartedAt >= _moveTimeout)
        {
            FailCurrentMove("Превышено время движения.");
            return;
        }

        if (_agent.pathPending)
            return;

        if (_agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            FailCurrentMove("Получен некорректный путь.");
            return;
        }

        if (_agent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            FailCurrentMove("До цели невозможно построить полный путь.");
            return;
        }

        float remainingDistance = _agent.remainingDistance;

        if (!float.IsInfinity(remainingDistance))
        {
            bool hasProgress =
                remainingDistance <
                _lastRemainingDistance - _progressEpsilon;

            if (hasProgress)
            {
                _lastRemainingDistance = remainingDistance;
                _lastProgressAt = Time.unscaledTime;
            }
        }

        bool isReached =
            remainingDistance <= _agent.stoppingDistance &&
            (!_agent.hasPath ||
             _agent.velocity.sqrMagnitude < 0.01f);

        if (isReached)
        {
            CompleteCurrentMove();
            return;
        }

        bool isNotMoving =
            _agent.velocity.sqrMagnitude < 0.001f;

        if (isNotMoving &&
            Time.unscaledTime - _lastProgressAt >= _stuckTimeout)
        {
            FailCurrentMove("Посетитель перестал продвигаться к цели.");
        }
    }

    // ИЗМЕНЕНО: при спавне вне NavMesh пытаемся аккуратно
    // переместить агента на ближайшую доступную область.
    private bool TryPlaceAgentOnNavMesh()
    {
        if (_agent == null || !_agent.enabled)
            return false;

        if (_agent.isOnNavMesh)
            return true;

        if (!NavMesh.SamplePosition(
                transform.position,
                out NavMeshHit hit,
                _navMeshSampleRadius * 2f,
                _agent.areaMask))
        {
            return false;
        }

        return _agent.Warp(hit.position);
    }

    private void CompleteCurrentMove()
    {
        Action complete = _onComplete;

        FinishMovement();
        RotateToTarget();

        complete?.Invoke();
    }

    private void FailCurrentMove(string reason)
    {
        Debug.LogWarning(
            $"VisitorMovement: движение {name} прервано. Причина: {reason}");

        Action failed = _onFailed;

        FinishMovement();
        failed?.Invoke();
    }

    private void FinishMovement()
    {
        _hasTarget = false;
        IsMoving = false;

        _onComplete = null;
        _onFailed = null;

        if (_agent != null &&
            _agent.enabled &&
            _agent.isOnNavMesh)
        {
            _agent.ResetPath();
            _agent.velocity = Vector3.zero;
            _agent.isStopped = true;
        }

        SetMovementAnimation(false);
    }

    private void CancelCurrentMove()
    {
        FinishMovement();
    }

    private void RotateToTarget()
    {
        Vector3 direction = _targetPos - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        transform.rotation =
            Quaternion.LookRotation(direction.normalized);
    }

    private void SetMovementAnimation(bool value)
    {
        if (_animator != null)
            _animator.SetBool(MovementParameter, value);
    }
}