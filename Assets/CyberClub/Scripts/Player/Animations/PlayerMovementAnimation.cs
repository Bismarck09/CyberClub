using UnityEngine;

public class PlayerMovementAnimation : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    private CharacterController _characterController;

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Play();
    }

    private void Play()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(_characterController.velocity);

        float velocityX = localVelocity.x;
        float velocityZ = localVelocity.z;

        velocityX = Mathf.Clamp(velocityX, -1f, 1f);
        velocityZ = Mathf.Clamp(velocityZ, -1f, 1f);

        _animator.SetFloat("VelocityX", velocityX);
        _animator.SetFloat("VelocityY", velocityZ);
    }
}
