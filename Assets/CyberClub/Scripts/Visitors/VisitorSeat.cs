using System;
using UnityEngine;
using UnityEngine.AI;

public class VisitorSeat : MonoBehaviour
{
    private const string MovementParameter = "IsMovement";
    private const string SittingParameter = "IsSitting";

    private VisitorMovement _movement;
    private NavMeshAgent _agent;
    private Animator _animator;

    private void Awake()
    {
        _movement = GetComponent<VisitorMovement>();
        _agent = GetComponent<NavMeshAgent>();

        _animator = GetComponent<Animator>();

        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();
    }

    public void SitAt(GameDevice device)
    {
        if (device == null || device.SitPoint == null)
        {
            Debug.LogError("VisitorSeatController: у устройства не назначен SitPoint.");
            return;
        }

        if (_movement != null)
            _movement.StopMovement();

        if (_agent != null && _agent.enabled)
        {
            _agent.ResetPath();
            _agent.velocity = Vector3.zero;
            _agent.isStopped = true;
            _agent.updatePosition = false;
            _agent.updateRotation = false;
        }

        transform.SetPositionAndRotation(device.SitPoint.position, device.SitPoint.rotation);

        if (_animator != null)
        {
            _animator.SetBool(MovementParameter, false);
            _animator.SetBool(SittingParameter, true);
        }
    }

    public void StandUp(Transform standPoint)
    {
        if (standPoint == null)
        {
            Debug.LogError("VisitorSeatController: у устройства не назначен TargetPoint.");
            return;
        }

        if (_animator != null)
        {
            _animator.SetBool(SittingParameter, false);
            _animator.SetBool(MovementParameter, false);
        }

        transform.SetPositionAndRotation(standPoint.position, standPoint.rotation);

        if (_agent != null && _agent.enabled)
        {
            _agent.updatePosition = true;
            _agent.updateRotation = true;
            _agent.isStopped = false;
            _agent.Warp(standPoint.position);
        }
    }
}