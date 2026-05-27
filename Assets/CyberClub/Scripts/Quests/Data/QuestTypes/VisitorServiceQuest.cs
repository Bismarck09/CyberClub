using UnityEngine;

public class VisitorServiceQuest : Quest
{
    private VisitorService _visitorService;
    
    public VisitorServiceQuest(VisitorService visitorService, IResource resource) :  base(QuestType.VisitorService, resource)
    {
        _visitorService = visitorService;
    }

    protected override void Subscribe()
    {
        _visitorService.OnVisitorServiced += Service;
    }

    protected override void Unsubscribe()
    {
        _visitorService.OnVisitorServiced -= Service;
    }

    private void Service(DeviceEntry device)
    {
        AddProgress(1);
    }
}
