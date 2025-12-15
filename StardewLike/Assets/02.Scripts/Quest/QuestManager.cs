using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager I { get; private set; }

    private Dictionary<string, QuestState> active = new();

    public List<QuestData> questDB;

    public event Action OnQuestChanged;

    public IReadOnlyDictionary<string, QuestState> Active => active;
    public QuestData GetQuestData(string questId) => GetData(questId);

    void Awake()
    {
        if(I != null) { Destroy(gameObject); return; }
        I = this;
    }
    public bool Accept(string questId)
    {
        if (active.ContainsKey(questId)) return false;

        var data = GetData(questId);
        if (data == null) return false;

        var state = new QuestState
        {
            questId = questId,
            accepted = true,
            completed = false,
            currentCounts = new List<int>(new int[data.objectives.Count])
        };

        active.Add(questId, state);
        OnQuestChanged?.Invoke();
        return true;
    }

    public void OnItemAdded(string itemId, int amount)
        => HandleEvent(ObjectiveType.Collect, itemId, amount);

    public void OnEnemyKilled(string enemyId, int amount = 1)
    {
        Debug.Log($"[QuestEvent] Kill {enemyId}");
        HandleEvent(ObjectiveType.Kill, enemyId, amount);
    }

    public void OnTalkedTo(string npcId)
    => HandleEvent(ObjectiveType.Talk, npcId, 1);

    public void OnEnterArea(string areaId)
    => HandleEvent(ObjectiveType.EnterArea, areaId, 1);

    public void HandleEvent(ObjectiveType type, string targetId, int delta)
    {
        bool anyChanged = false;

        foreach (var kv in active)
        {
            var state = kv.Value;
            if (state.completed) continue;

            var data = GetData(state.questId);
            if (data == null) continue;

            bool changed = false;

            for (int i = 0; i < data.objectives.Count; i++)
            {
                var obj = data.objectives[i];
                if (obj.type != type) continue;
                if (obj.targetId != targetId) continue;

                int cur = state.currentCounts[i];
                int next = Mathf.Min(obj.requiredCount, cur + delta);
                if (next != cur)
                {
                    state.currentCounts[i] = next;
                    changed = true;
                }
            }

            if (changed && IsQuestComplete(data, state))
            {
                CompleteQuest(data, state);
            }
        }

        if (anyChanged) OnQuestChanged?.Invoke();
    }

    // 퀘스트 목표 갱신
    private bool IsQuestComplete(QuestData data, QuestState state)
    {
        for (int i = 0; i < data.objectives.Count; i++)
        {
            if (state.currentCounts[i] < data.objectives[i].requiredCount)
                return false;
        }
        return true;
    }

    private void CompleteQuest(QuestData data, QuestState state)
    {
        state.completed = true;
        Debug.Log($"[Quest] Completed: {data.title}");
    }

    private QuestData GetData(string questId)
    => questDB.Find(q => q.questId == questId);
}
