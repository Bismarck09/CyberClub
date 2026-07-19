using System;
using UnityEngine;

public class VisitorExit : MonoBehaviour
{
    [SerializeField] private Vector3 _exitPoint;

    private VisitorMovement _visitorMovement;
    private bool _hasExited;
    private bool _isApplicationQuitting;
    private GameDevice _reservedDevice;

    public event Action OnVisitorExit;

    public void TrackReservedDevice(GameDevice device)
    {
        _reservedDevice = device;
    }

    public void ClearReservedDevice(GameDevice device)
    {
        if (_reservedDevice == device)
            _reservedDevice = null;
    }

    public void ReleaseReservedDevice()
    {
        // ИЗМЕНЕНО: Unity-ссылка очищается до вызова и проверяется через
        // перегруженный оператор UnityEngine.Object, а не через C# null-conditional.
        GameDevice device = _reservedDevice;
        _reservedDevice = null;

        if (_isApplicationQuitting)
            return;

        if (device != null)
            device.Release();
    }

    // ИЗМЕНЕНО: Awake вместо Start, чтобы ссылка была доступна
    // даже при ранней ошибке регистрации.
    private void Awake()
    {
        _visitorMovement = GetComponent<VisitorMovement>();
    }

    public void MoveToExit()
    {
        if (_hasExited)
            return;

        if (_visitorMovement == null)
        {
            ExitImmediately();
            return;
        }

        bool movementStarted = _visitorMovement.Move(
            _exitPoint,
            FinishExit,
            ExitImmediately);

        if (!movementStarted)
            ExitImmediately();
    }

    // ИЗМЕНЕНО: корректно завершает жизненный цикл,
    // даже если посетитель не может использовать NavMesh.
    public void ExitImmediately()
    {
        FinishExit();
    }

    private void FinishExit()
    {
        if (_hasExited)
            return;

        _hasExited = true;
        ReleaseReservedDevice();

        Debug.Log($"Visitor exited: {name}");

        NotifyVisitorExitSafely();
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (_isApplicationQuitting)
        {
            _reservedDevice = null;
            return;
        }

        ReleaseReservedDevice();

        // ИЗМЕНЕНО: внешнее уничтожение посетителя тоже освобождает лимит спавнера.
        if (_hasExited)
            return;

        _hasExited = true;
        NotifyVisitorExitSafely();
    }

    private void OnApplicationQuit()
    {
        _isApplicationQuitting = true;
        _reservedDevice = null;
    }

    private void NotifyVisitorExitSafely()
    {
        if (OnVisitorExit == null)
            return;

        // ИЗМЕНЕНО: сбой одного подписчика не мешает спавнеру уменьшить счётчик посетителей.
        foreach (Delegate handler in OnVisitorExit.GetInvocationList())
        {
            try
            {
                ((Action)handler).Invoke();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }
    }
}
