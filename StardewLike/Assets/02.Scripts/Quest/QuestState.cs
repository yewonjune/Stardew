using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestState
{
    public string questId;
    public bool accepted;
    public bool completed;

    public List<int> currentCounts = new();
}
