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
        MoveQueue();
    }

    public Visitor GetNextVisitor()
    {
        if (_queue.Count == 0)
            return null;

        Visitor nextVisitor = _queue[0];

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
            if (_queue[i] == null)
                continue;

            VisitorMovement visitor = _queue[i].GetComponent<VisitorMovement>();

            if (Vector3.Distance(visitor.transform.position, _queuePoints[i].position) < 0.1f)
                continue;

            visitor.Move(_queuePoints[i].position);
        }
    }
}
