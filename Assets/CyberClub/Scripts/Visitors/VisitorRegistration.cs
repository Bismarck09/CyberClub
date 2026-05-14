using UnityEngine;

public class VisitorRegistration : MonoBehaviour
{
    private VisitorQueue _visitorQueue;
    private Transform _registrationPoint;
    private VisitorMovement _visitorMovement;

    public bool IsRegistered { get; private set; }

    public void Init(VisitorMovement visitorMovement, VisitorQueue visitorQueue)
    {
        _visitorMovement = visitorMovement;
        _visitorQueue = visitorQueue;
        IsRegistered = false;

        MoveToQueue();
    }

    private void MoveToQueue()
    {
        _registrationPoint = _visitorQueue.GetNextQueuePoint(GetComponent<Visitor>());

        if (_registrationPoint != null)
            _visitorMovement.Move(_registrationPoint.position, RegisterVisitor);
    }

    private void RegisterVisitor()
    {
        IsRegistered = true;
    }
}
