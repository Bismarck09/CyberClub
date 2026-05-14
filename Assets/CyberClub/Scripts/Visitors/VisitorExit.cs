using System;
using UnityEngine;

public class VisitorExit : MonoBehaviour
{
    [SerializeField] private Vector3 _exitPoint;
    
    private VisitorMovement _visitorMovement;

    public event Action OnVisitorExit;

    private void Start()
    {
        _visitorMovement = GetComponent<VisitorMovement>();
    }

    public void MoveToExit()
    {
        if (_exitPoint != null)
            _visitorMovement.Move(_exitPoint, Exit);
    }

    private void Exit()
    {
        Debug.Log("Visitor exited");
        OnVisitorExit?.Invoke();
        Destroy(gameObject);
    }
}
