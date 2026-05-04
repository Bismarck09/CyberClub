using UnityEngine;

public class VisitorRegistration : MonoBehaviour
{
    private VisitorQueue _visitorQueue;
    private Transform _registrationPoint;
    private VisitorMovement _visitorMovement;

    public void Init(Transform registrationPoint, VisitorMovement visitorMovement, VisitorQueue visitorQueue)
    {
        _registrationPoint = registrationPoint;
        _visitorMovement = visitorMovement;
        _visitorQueue = visitorQueue;

        _visitorMovement.Move(_registrationPoint.position);
        _visitorQueue.AddToQueue(GetComponent<Visitor>());
    }
}
