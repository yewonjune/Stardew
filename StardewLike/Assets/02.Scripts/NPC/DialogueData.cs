using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "NPC/Dialogue")]
public class DialogueData : ScriptableObject
{
    public string npcName;
    [TextArea(3, 5)]
    public string[] lines;
}
