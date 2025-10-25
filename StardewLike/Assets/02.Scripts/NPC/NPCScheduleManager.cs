using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCScheduleManager : MonoBehaviour
{
    public TimeManager timeManager;
    public AStarGrid2D grid;

    readonly HashSet<(int minuteStamp, GameObject npc)> _fired = new();

    void OnEnable()
    {
        if (timeManager) timeManager.OnMinuteChanged += OnMinuteChanged;
    }

    void OnDisable()
    {
        if (timeManager) timeManager.OnMinuteChanged -= OnMinuteChanged;
    }

    void OnMinuteChanged(int hour, int minute)
    {
        int stamp = hour * 60 + minute;

        var npcs = FindObjectsOfType<NPCScheduleHolder>(includeInactive: false);
        foreach (var holder in npcs)
        {
            var npcGo = holder.gameObject;
            var key = (stamp, npcGo);
            if (_fired.Contains(key)) continue;

            foreach (var s in holder.schedules)
            {
                if (s.hour == hour && s.minute == minute && s.target)
                {
                    var agent = npcGo.GetComponent<NPCPathAgent2D>();
                    if (!agent) agent = npcGo.AddComponent<NPCPathAgent2D>();
                    agent.SetGrid(grid);
                    bool ok = agent.SetDestination(s.target.position);

                    _fired.Add(key);
                    // 필요하면 실패 로깅
                    if (!ok) Debug.Log($"[NPCSchedule] 경로 실패: {npcGo.name} -> {s.target.name}");
                }
            }
        }
    }

        // Update is called once per frame
        void Update()
    {
        NPCMovement[] npcs = FindObjectsOfType<NPCMovement>();

        foreach (NPCMovement npc in npcs)
        {
            var holder = npc.GetComponent<NPCScheduleHolder>();
            if (holder == null) continue;

            foreach (var schedule in holder.schedules)
            {
                if (timeManager.hour == schedule.hour && timeManager.minute == schedule.minute)
                {
                    npc.SetTarget(schedule.target.position);
                }
            }
        }
    }
}
