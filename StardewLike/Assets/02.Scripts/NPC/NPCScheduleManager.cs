using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCScheduleManager : MonoBehaviour
{
    public TimeManager timeManager;

    private readonly List<NPCScheduleHolder> npcs = new();

    void OnEnable()
    {
        if (timeManager == null)
        {
            timeManager = FindObjectOfType<TimeManager>();
        }

        if (timeManager != null)
        {
            timeManager.OnMinuteChanged -= OnMinuteChanged;
            timeManager.OnMinuteChanged += OnMinuteChanged;
        }
    }

    void OnDisable()
    {
        if (timeManager != null)
        {
            timeManager.OnMinuteChanged -= OnMinuteChanged;
        }
    }
    public void Register(NPCScheduleHolder holder)
    {
        if (holder == null) return;
        if (!npcs.Contains(holder)) npcs.Add(holder);
    }

    public void Unregister(NPCScheduleHolder holder)
    {
        if (holder == null) return;
        npcs.Remove(holder);
    }

    void OnMinuteChanged(int hour, int minute)
    {
        for (int i = 0; i < npcs.Count; i++)
        {
            var holder = npcs[i];
            if (holder == null || holder.schedules == null) continue;

            foreach (var entry in holder.schedules)
            {
                if (entry.hour == hour && entry.minute == minute &&
                    holder.movement != null && entry.path != null && entry.path.Length > 0)
                {
                    holder.movement.SetPath(entry.path, false);
                    break;
                }
            }
        }
    }
}
