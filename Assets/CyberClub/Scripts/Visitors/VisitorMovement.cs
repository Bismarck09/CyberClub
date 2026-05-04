using UnityEngine;
using UnityEngine.AI;

public class VisitorMovement : MonoBehaviour
{
    private NavMeshAgent _agent;
    private Vector3 _targetPos;
    private bool _hasTarget;

    public bool IsMoving { get; private set; }

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (!_hasTarget)
            return;

        if (_agent.velocity == Vector3.zero)
        {
            IsMoving = false;
            RotateToTarget();
        }
        else
            IsMoving = true;
    }

    public void Move(Vector3 target)
    {
        _targetPos = target;
        _agent.SetDestination(_targetPos);
        _hasTarget = true;
    }

    private void RotateToTarget()
    {
        Vector3 direction = (_targetPos - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
}
