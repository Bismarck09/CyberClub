using Unity.Cinemachine;
using UnityEngine;

public class PlayerRotation : MonoBehaviour
{
    [SerializeField] private CinemachineInputAxisController _cinemachineInputAxisController;
    [SerializeField] private Transform _playerHead;
    [SerializeField] private InteractionWithUI _interactionWithUI;
    [SerializeField] private float _sensitivity;
    [SerializeField] private float _maxRotationX;
    [SerializeField] private float _minRotationX;
    [SerializeField] private PlayerInputReader _inputReader;

    private float _rotationX;
    private float _rotationY;

    private bool _isRotateActive;

    private void Awake()
    {
        if (_inputReader == null)
            _inputReader = GetComponent<PlayerInputReader>();
    }

    private void OnEnable()
    {
        if (_interactionWithUI != null)
            _interactionWithUI.IsInteractsChanged += SwitchRotateActive;
    }

    private void OnDisable()
    {
        if (_interactionWithUI != null)
            _interactionWithUI.IsInteractsChanged -= SwitchRotateActive;
    }

    private void Update()
    {
        if (_isRotateActive)
            Rotate();
    }

    private void Rotate()
    {
        if (_inputReader == null)
            return;

        Vector2 rotateDirection = _inputReader.Look;

        _rotationX -= rotateDirection.y * _sensitivity * Time.deltaTime;
        _rotationY += rotateDirection.x * _sensitivity * Time.deltaTime;

        _rotationX = Mathf.Clamp(_rotationX, _minRotationX, _maxRotationX);

        _playerHead.localRotation = Quaternion.Euler(_rotationX, 0f, 0f);
        transform.localRotation = Quaternion.Euler(0f, _rotationY, 0f);
    }

    private void SwitchRotateActive(bool isActive)
    {
        _isRotateActive = !isActive;
        if (_cinemachineInputAxisController != null)
            _cinemachineInputAxisController.enabled = _isRotateActive;
        Debug.Log($"Rotate active: {_isRotateActive}");
    }
}
