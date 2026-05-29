using System.Collections.Generic;
using UnityEngine;

public class VisitorQueue : MonoBehaviour
{
    [SerializeField] private List<AdminWorker> _admins;

    public bool HasFreeSlot()
    {
        foreach (AdminWorker admin in _admins)
        {
            if (admin.HasFreeQueueSlot())
                return true;
        }

        return false;
    }

    public Transform GetNextQueuePoint(Visitor visitor)
    {
        AdminWorker admin = GetBestAdminForQueue();

        if (admin == null)
            return null;

        return admin.AddVisitorToQueue(visitor);
    }

    public Visitor GetNextVisitor(AdminWorker admin)
    {
        if (admin == null)
            return null;

        return admin.GetNextVisitor();
    }

    public void RemoveVisitor(AdminWorker admin, Visitor visitor)
    {
        if (admin == null)
            return;

        admin.RemoveVisitor(visitor);
    }

    public List<AdminWorker> GetAdmins()
    {
        return _admins;
    }

    private AdminWorker GetBestAdminForQueue()
    {
        AdminWorker bestAdmin = null;

        foreach (AdminWorker admin in _admins)
        {
            if (!admin.HasFreeQueueSlot())
                continue;

            if (bestAdmin == null || admin.QueueCount < bestAdmin.QueueCount)
                bestAdmin = admin;
        }

        return bestAdmin;
    }
}
