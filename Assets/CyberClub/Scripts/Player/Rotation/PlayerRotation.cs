using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRotation : MonoBehaviour
{
    [SerializeField] private Transform _playerHead;
    [SerializeField] private InteractionWithUI _interactionWithUI;
    [SerializeField] private float _sensitivity;
    [SerializeField] private float _maxRotationX;
    [SerializeField] private float _minRotationX;

    private float _rotationX;
    private float _rotationY;

    private bool _isRotateActive;

    private PlayerInput _playerInput;
    private InputAction _inputAction;

    private void Start()
    {
        _playerInput = GetComponent<PlayerInput>();
        _inputAction = _playerInput.actions["Look"];
    }

    private void OnEnable() => _interactionWithUI.IsInteractsChanged += SwitchRotateActive;
    private void OnDisable() => _interactionWithUI.IsInteractsChanged -= SwitchRotateActive;

    private void Update()
    {
        if (_isRotateActive)
            Rotate();
    }

    private void Rotate()
    {
        Vector2 rotateDirection = _inputAction.ReadValue<Vector2>();
        Debug.Log($"Rotate direction: {rotateDirection}");

        _rotationX -= rotateDirection.y * _sensitivity * Time.deltaTime;
        _rotationY += rotateDirection.x * _sensitivity * Time.deltaTime;

        _rotationX = Mathf.Clamp(_rotationX, _minRotationX, _maxRotationX);

        _playerHead.localRotation = Quaternion.Euler(_rotationX, 0f, 0f);
        transform.localRotation = Quaternion.Euler(0f, _rotationY, 0f);
    }

    private void SwitchRotateActive(bool isActive)
    {
        _isRotateActive = !isActive;
        Debug.Log($"Rotate active: {_isRotateActive}");
    }
}
