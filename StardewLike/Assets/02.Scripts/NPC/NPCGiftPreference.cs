using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "NPC/NPC Gift Preference")]
public class NPCGiftPreference : ScriptableObject
{
    public string npcId;

    [Header("사랑하는 선물")]
    public string[] lovedItemIds;

    [Header("좋아하는 선물")]
    public string[] likedItemIds;

    [Header("싫어하는 선물")]
    public string[] dislikedItemIds;

    public enum GiftResult
    {
        Loved,
        Liked,
        Disliked,
        Neutral
    }

    public GiftResult Evaluate(string itemId)
    {
        // 사랑
        foreach (var id in lovedItemIds)
        {
            if (id == itemId) return GiftResult.Loved;
        }

        // 좋아
        foreach (var id in likedItemIds)
        {
            if (id == itemId) return GiftResult.Liked;
        }

        // 싫어
        foreach (var id in dislikedItemIds)
        {
            if (id == itemId) return GiftResult.Disliked;
        }

        return GiftResult.Neutral;
    }
}
