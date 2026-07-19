using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _acceleration;
    [SerializeField, Min(1f)] private float _sprintMultiplier = 1.5f;
    [SerializeField] private PlayerInputReader _inputReader;

    private Vector3 _currentVelocity;
    private CharacterController _characterController;

    private void Awake()
    {
        if (_inputReader == null)
            _inputReader = GetComponent<PlayerInputReader>();
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        if (_inputReader == null || _characterController == null)
            return;

        Vector2 input = _inputReader.Movement;
        Vector3 direction = new Vector3(input.x, 0, input.y);

        Vector3 worldDirection = transform.TransformDirection(direction);

        float speed = _moveSpeed * (_inputReader.Sprint ? _sprintMultiplier : 1f);
        _currentVelocity = Vector3.Lerp(_currentVelocity, worldDirection * speed, _acceleration * Time.deltaTime);

        _characterController.Move(_currentVelocity * Time.deltaTime);

    }
}
