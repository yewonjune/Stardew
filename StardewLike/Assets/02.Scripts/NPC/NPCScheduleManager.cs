using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCScheduleManager : MonoBehaviour
{
    public TimeManager timeManager;
    public NPCScheduleHolder[] npcs;

    void OnEnable()
    {
        if (timeManager == null)
        {
            timeManager = FindObjectOfType<TimeManager>();
        }

        if (timeManager != null)
        {
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

    void OnMinuteChanged(int hour, int minute)
    {
        // 모든 NPC 검사
        foreach (var holder in npcs)
        {
            if (holder == null || holder.schedules == null) continue;

            foreach (var entry in holder.schedules)
            {
                    if (entry.hour == hour && entry.minute == minute && holder.movement != null && entry.path != null && entry.path.Length > 0)
                    {
                        holder.movement.SetPath(entry.path, false);
                        break;
                    }
            }
        }
    }
}
