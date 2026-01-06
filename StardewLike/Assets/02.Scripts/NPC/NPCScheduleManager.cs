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

            OnMinuteChanged(timeManager.hour, timeManager.minute);
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
        int now = hour * 60 + minute;

        for (int i = 0; i < npcs.Count; i++)
        {
            var holder = npcs[i];
            if (holder == null || holder.schedules == null || holder.movement == null) continue;

            Schedule best = null;
            int bestTime = -1;

            foreach (var entry in holder.schedules)
            {
                if (entry.path == null || entry.path.Length == 0) continue;

                int t = entry.hour * 60 + entry.minute;
                if (t <= now && t > bestTime)
                {
                    bestTime = t;
                    best = entry;
                }
            }

            if (best == null) continue;

            if (holder.lastAppliedBestTime == bestTime) continue;

            holder.lastAppliedBestTime = bestTime;
            holder.movement.SetPath(best.path, false);
        }
    }
}
