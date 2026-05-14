using UnityEngine;
using System.Collections.Generic;

public class VisitorQueue : MonoBehaviour
{
    [SerializeField] private Transform[] _queuePoints;

    private List<Visitor> _queue = new ();

    public bool HasFreeSlot()
    {
        return _queue.Count < _queuePoints.Length;
    }

    public void AddToQueue(Visitor visitor)
    {
        _queue.Add(visitor);
    }

    public Transform GetNextQueuePoint(Visitor visitor)
    {
        AddToQueue(visitor);

        if (_queue.Count >= _queuePoints.Length)
            return null;


        return _queuePoints[_queue.Count - 1];
    }

    public Visitor GetNextVisitor()
    {
        if (_queue.Count == 0)
            return null;

        Visitor nextVisitor = _queue[0].GetComponent<VisitorRegistration>().IsRegistered ? _queue[0] : null;

        return nextVisitor;
    }

    public void RemoveVisitor(Visitor visitor)
    {
        if (_queue.Contains(visitor))
        {
            _queue.Remove(visitor);
            MoveQueue();
        }
    }

    private void MoveQueue()
    {
        for (int i = 0; i < _queue.Count; i++)
        {
            if (_queue[i] == null || _queue[i].GetComponent<VisitorRegistration>().IsRegistered == false)
                continue;

            VisitorMovement visitor = _queue[i].GetComponent<VisitorMovement>();

            if (Vector3.Distance(visitor.transform.position, _queuePoints[i].position) < 0.1f)
                continue;

            visitor.Move(_queuePoints[i].position);
        }
    }
}
