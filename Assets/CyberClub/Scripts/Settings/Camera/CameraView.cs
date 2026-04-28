using Unity.Cinemachine;
using UnityEngine;

public class CameraView : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _firstCamera;
    [SerializeField] private CinemachineCamera _thirdCamera;
    [SerializeField] private Animator _animator;


    void Awake()
    {
        _thirdCamera.Priority = 1;
        _animator.enabled = true;
    }

    public void SwitchCameraView()
    {
        _animator.enabled = !_animator.enabled;

        int priority = _thirdCamera.Priority;
        _thirdCamera.Priority = _firstCamera.Priority;
        _firstCamera.Priority = priority;

    }
}
