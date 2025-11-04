using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [TextArea(2, 5)]
    public string text;
    public EmotionType emotion;
}

[System.Serializable]
public class DialogueSequence
{
    public DialogueLine[] lines;
}


[CreateAssetMenu(fileName = "New Dialogue", menuName = "NPC/Dialogue")]
public class DialogueData : ScriptableObject
{
    public NPCData npcData;
    public DialogueSequence[] sequences;

    public DialogueSequence loveGiftSequence;
    public DialogueSequence likeGiftSequence;
    public DialogueSequence dislikeGiftSequence;
    public DialogueSequence neutralGiftSequence;

}
