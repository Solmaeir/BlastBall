using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Ekran noktasının bir UI öğesinin üzerinde olup olmadığını söyler. Ortak kullanım için tek örnek tutar.
public static class PointerOverUI
{
    private static readonly List<RaycastResult> Hits = new List<RaycastResult>();
    private static PointerEventData data;
    private static EventSystem owner;

    public static bool Check(Vector2 screen)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) return false;

        if (data == null || owner != eventSystem)
        {
            owner = eventSystem;
            data = new PointerEventData(eventSystem);
        }

        data.position = screen;
        Hits.Clear();
        eventSystem.RaycastAll(data, Hits);
        return Hits.Count > 0;
    }
}
