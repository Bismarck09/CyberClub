using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class VisitorSpeedBoostAdapter : MonoBehaviour
{
    private NavMeshAgent _agent;
    private SpeedPotionEffectService _speedService;
    private float _baseSpeed;
    private bool _isInitialized;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();

        if (_agent != null)
        {
            _baseSpeed = _agent.speed;
            _isInitialized = true;
        }
    }

    private void OnEnable()
    {
        Subscribe();
        ApplySpeed();
    }

    private void OnDisable()
    {
        Unsubscribe();

        if (_agent != null && _isInitialized)
            _agent.speed = _baseSpeed;
    }

    private void Update()
    {
        // Fallback на случай, если посетитель появился раньше SpeedPotionEffectService.
        if (_speedService == null && SpeedPotionEffectService.Current != null)
        {
            Subscribe();
            ApplySpeed();
        }
    }

    private void Subscribe()
    {
        if (_speedService == SpeedPotionEffectService.Current)
            return;

        Unsubscribe();

        _speedService = SpeedPotionEffectService.Current;

        if (_speedService != null)
            _speedService.OnChanged += ApplySpeed;
    }

    private void Unsubscribe()
    {
        if (_speedService != null)
            _speedService.OnChanged -= ApplySpeed;

        _speedService = null;
    }

    private void ApplySpeed()
    {
        if (_agent == null || !_isInitialized)
            return;

        float multiplier = _speedService != null ? _speedService.VisitorMovementMultiplier : 1f;
        _agent.speed = _baseSpeed * Mathf.Max(1f, multiplier);
    }
}
