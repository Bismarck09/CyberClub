using System.Collections.Generic;
using UnityEngine;

public class VisitorQueue : MonoBehaviour
{
    [SerializeField] private List<AdminWorker> _admins;

    // ИЗМЕНЕНО: спавнер теперь может узнать точное количество
    // свободных мест, а не только true/false.
    public int FreeSlotCount
    {
        get
        {
            if (_admins == null)
                return 0;

            int freeSlots = 0;

            foreach (AdminWorker admin in _admins)
            {
                if (admin == null)
                    continue;

                freeSlots += admin.FreeQueueSlotCount;
            }

            return freeSlots;
        }
    }

    public bool HasFreeSlot()
    {
        return FreeSlotCount > 0;
    }

    // ИЗМЕНЕНО: атомарно пытаемся зарезервировать место.
    public bool TryGetNextQueuePoint(
        Visitor visitor,
        out Transform queuePoint)
    {
        queuePoint = null;

        if (visitor == null)
            return false;

        AdminWorker admin = GetBestAdminForQueue();

        if (admin == null)
            return false;

        queuePoint = admin.AddVisitorToQueue(visitor);
        return queuePoint != null;
    }

    // Оставлено для совместимости со старым кодом.
    public Transform GetNextQueuePoint(Visitor visitor)
    {
        return TryGetNextQueuePoint(visitor, out Transform queuePoint)
            ? queuePoint
            : null;
    }

    public Visitor GetNextVisitor(AdminWorker admin)
    {
        if (admin == null)
            return null;

        return admin.GetNextVisitor();
    }

    public void RemoveVisitor(AdminWorker admin, Visitor visitor)
    {
        if (admin == null || visitor == null)
            return;

        admin.RemoveVisitor(visitor);
    }

    // ИЗМЕНЕНО: позволяет удалить посетителя,
    // даже если вызывающий код не знает его администратора.
    public void RemoveVisitor(Visitor visitor)
    {
        if (visitor == null || _admins == null)
            return;

        foreach (AdminWorker admin in _admins)
        {
            if (admin != null)
                admin.RemoveVisitor(visitor);
        }
    }

    public List<AdminWorker> GetAdmins()
    {
        return _admins;
    }

    private AdminWorker GetBestAdminForQueue()
    {
        if (_admins == null)
            return null;

        AdminWorker bestAdmin = null;

        foreach (AdminWorker admin in _admins)
        {
            if (admin == null || !admin.HasFreeQueueSlot())
                continue;

            if (bestAdmin == null || admin.QueueCount < bestAdmin.QueueCount)
                bestAdmin = admin;
        }

        return bestAdmin;
    }
}