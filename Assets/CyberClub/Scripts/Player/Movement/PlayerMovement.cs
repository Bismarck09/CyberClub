using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _acceleration;

    private Vector3 _currentVelocity;
    private CharacterController _characterController;
    private PlayerInput _playerInput;
    private InputAction inputAction;

    private void Start()
    {
        _playerInput = GetComponent<PlayerInput>();
        inputAction = _playerInput.actions["Move"];

        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        Vector2 input = inputAction.ReadValue<Vector2>();
        Vector3 direction = new Vector3(input.x, 0, input.y);

        Vector3 worldDirection = transform.TransformDirection(direction);

        _currentVelocity = Vector3.Lerp(_currentVelocity, worldDirection * _moveSpeed, _acceleration * Time.deltaTime);

        _characterController.Move(_currentVelocity * Time.deltaTime);

    }
}
