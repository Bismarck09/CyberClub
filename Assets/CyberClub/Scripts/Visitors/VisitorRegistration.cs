using UnityEngine;

public class VisitorRegistration : MonoBehaviour
{
    private VisitorQueue _visitorQueue;
    private Transform _registrationPoint;
    private VisitorMovement _visitorMovement;
    private Visitor _visitor;

    public bool IsRegistered { get; private set; }

    // ИЗМЕНЕНО: теперь возвращает результат регистрации.
    public bool Init(
        VisitorMovement visitorMovement,
        VisitorQueue visitorQueue)
    {
        _visitorMovement = visitorMovement;
        _visitorQueue = visitorQueue;
        _visitor = GetComponent<Visitor>();

        IsRegistered = false;

        if (_visitorMovement == null ||
            _visitorQueue == null ||
            _visitor == null)
        {
            Debug.LogError(
                $"VisitorRegistration: у {name} отсутствуют обязательные компоненты.");

            return false;
        }

        return MoveToQueue();
    }

    private bool MoveToQueue()
    {
        // ИЗМЕНЕНО: если места нет, посетитель не остаётся
        // навсегда стоять в точке спавна.
        if (!_visitorQueue.TryGetNextQueuePoint(
                _visitor,
                out _registrationPoint))
        {
            return false;
        }

        bool movementStarted = _visitorMovement.Move(
            _registrationPoint.position,
            RegisterVisitor,
            HandleQueueMovementFailed);

        if (movementStarted)
            return true;

        _visitorQueue.RemoveVisitor(_visitor);
        return false;
    }

    private void RegisterVisitor()
    {
        IsRegistered = true;
        GetComponent<VisitorRatingTracker>()?.StartWaiting();
    }

    private void HandleQueueMovementFailed()
    {
        IsRegistered = false;

        Debug.LogWarning(
            $"VisitorRegistration: {name} не смог дойти до очереди и покидает систему.",
            this);

        if (_visitorQueue != null && _visitor != null)
            _visitorQueue.RemoveVisitor(_visitor);

        VisitorExit visitorExit = GetComponent<VisitorExit>();

        if (visitorExit != null)
            visitorExit.ExitImmediately();
        else
            Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (_visitorQueue != null && _visitor != null)
            _visitorQueue.RemoveVisitor(_visitor);
    }
}
