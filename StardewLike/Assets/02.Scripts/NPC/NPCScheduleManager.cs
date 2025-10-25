using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCScheduleManager : MonoBehaviour
{
    public TimeManager timeManager;

    readonly HashSet<(int minuteStamp, GameObject npc)> _fired = new();

    void OnEnable()
    {
        if (timeManager)
        {
            timeManager.OnMinuteChanged += OnMinuteChanged;
            //timeManager.OnDayChanged += OnDayChanged; // 있으면 연결
        }
    }

    void OnDisable()
    {
        if (timeManager)
        {
            timeManager.OnMinuteChanged -= OnMinuteChanged;
            //timeManager.OnDayChanged -= OnDayChanged;
        }
    }

    void OnDayChanged(int newDay)
    {
        _fired.Clear();
    }


    void OnMinuteChanged(int hour, int minute)
    {
        int stamp = hour * 60 + minute;

        var holders = FindObjectsOfType<NPCScheduleHolder>(includeInactive: false);
        Debug.Log($"[Sched] {hour:D2}:{minute:D2} holders={holders.Length}");

        foreach (var holder in holders)
        {
            var npcGo = holder.gameObject;
            var key = (stamp, npcGo);
            if (_fired.Contains(key)) continue;

            foreach (var s in holder.schedules)
            {
                if (s.hour == hour && s.minute == minute && s.target)
                {
                    Debug.Log($"[Sched] match -> {holder.name} -> {s.target?.name}");

                    _fired.Add(key); // 같은 분에 같은 NPC는 1회만

                }
            }
        }
    }
       
}
