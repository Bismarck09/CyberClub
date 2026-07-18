using System;
using UnityEngine;

public class VisitorExit : MonoBehaviour
{
    [SerializeField] private Vector3 _exitPoint;

    private VisitorMovement _visitorMovement;
    private bool _hasExited;

    public event Action OnVisitorExit;

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

        Debug.Log($"Visitor exited: {name}");

        OnVisitorExit?.Invoke();
        Destroy(gameObject);
    }
}