using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class MobilePointerUiGuard
{
    private static readonly List<RaycastResult> RaycastResults = new(32);

    public static bool IsPointerOverInteractiveUi(
        Vector2 screenPosition,
        int pointerId,
        Transform ignoredRoot = null)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
            return false;

        PointerEventData eventData = new(eventSystem)
        {
            position = screenPosition,
            pointerId = pointerId
        };

        RaycastResults.Clear();
        eventSystem.RaycastAll(eventData, RaycastResults);

        for (int i = 0; i < RaycastResults.Count; i++)
        {
            GameObject target = RaycastResults[i].gameObject;
            if (target == null || IsIgnored(target.transform, ignoredRoot))
                continue;

            if (target.GetComponentInParent<Selectable>() != null ||
                target.GetComponentInParent<ScrollRect>() != null ||
                target.GetComponentInParent<EventTrigger>() != null ||
                HasPointerHandler(target))
            {
                RaycastResults.Clear();
                return true;
            }
        }

        RaycastResults.Clear();
        return false;
    }

    private static bool HasPointerHandler(GameObject target)
    {
        return ExecuteEvents.GetEventHandler<IPointerClickHandler>(target) != null ||
            ExecuteEvents.GetEventHandler<IPointerDownHandler>(target) != null ||
            ExecuteEvents.GetEventHandler<IBeginDragHandler>(target) != null ||
            ExecuteEvents.GetEventHandler<IDragHandler>(target) != null ||
            ExecuteEvents.GetEventHandler<IScrollHandler>(target) != null;
    }

    private static bool IsIgnored(Transform target, Transform ignoredRoot)
    {
        return ignoredRoot != null &&
            (target == ignoredRoot || target.IsChildOf(ignoredRoot));
    }
}
