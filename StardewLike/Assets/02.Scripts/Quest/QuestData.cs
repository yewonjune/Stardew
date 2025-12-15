using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName ="Quest/QuestData")]
public class QuestData : ScriptableObject
{
    public string questId;
    public string title;
    [TextArea] public string description;

    public List<ObjectiveData> objectives;
    public RewardData reward;
}

[System.Serializable]
public class RewardData
{
    public int gold;
}

public enum ObjectiveType { Collect, Kill, Talk, EnterArea}

[System.Serializable]
public class ObjectiveData
{
    public ObjectiveType type;
    public string targetId;
    public int requiredCount;
}
