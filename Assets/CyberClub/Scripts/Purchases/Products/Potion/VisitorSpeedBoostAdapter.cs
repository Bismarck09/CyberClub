using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class VisitorSpeedBoostAdapter : MonoBehaviour
{
    private NavMeshAgent _agent;
    private float _baseSpeed;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _baseSpeed = _agent.speed;
    }

    private void Update()
    {
        if (_agent == null)
            return;

        float multiplier = SpeedPotionEffectService.Current != null
            ? SpeedPotionEffectService.Current.VisitorMovementMultiplier
            : 1f;

        _agent.speed = _baseSpeed * Mathf.Max(1f, multiplier);
    }
}