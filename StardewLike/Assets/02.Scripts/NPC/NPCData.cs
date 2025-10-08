using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EmotionType
{
    Default,
    Happy,
    Angry,
    Surprised
}

[CreateAssetMenu(menuName = "NPC/NPC Data")]
public class NPCData : ScriptableObject
{
    public string npcId;
    public string displayName;

    public Sprite defaultPortrait;
    public Sprite happyPortrait;
    public Sprite angryPortrait;
    public Sprite surprisedPortrait;

    public Sprite GetPortrait(EmotionType type)
    {
        switch(type)
        {
            case EmotionType.Happy:
                return happyPortrait;            
            case EmotionType.Angry:
                return angryPortrait;
            case EmotionType.Surprised:
                return surprisedPortrait;
            default:
                return defaultPortrait;
        }
    }

}
