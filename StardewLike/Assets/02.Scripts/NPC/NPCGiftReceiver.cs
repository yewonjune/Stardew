using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCGiftReceiver : MonoBehaviour
{
    public NPCGiftPreference giftPreference;
    public DialogueData dialogueData;

    [Title("NPC 호감도")]
    [FoldoutGroup("NPC 호감도")]
    [LabelText("기본 호감도")]
    public int baseAffectionOnGift = 5;

    [FoldoutGroup("NPC 호감도")]
    [LabelText("사랑하는 선물 호감도")]
    public int lovedBonus = 15;

    [FoldoutGroup("NPC 호감도")]
    [LabelText("좋아하는 선물 호감도")]
    public int likedBonus = 8;

    [FoldoutGroup("NPC 호감도")]
    [LabelText("싫어하는 선물 호감도")]
    public int dislikedPenalty = -5;

    NPCAffection affection;

    void Awake()
    {
        affection = GetComponent<NPCAffection>();
    }

    public void ReceiveGift(string itemId)
    {
        int add = 0;

        var result = NPCGiftPreference.GiftResult.Neutral;
        if (giftPreference)
            result = giftPreference.Evaluate(itemId);

        PlayGiftDialogue(result);

        if (result == NPCGiftPreference.GiftResult.Loved)
            add += lovedBonus;
        else if (result == NPCGiftPreference.GiftResult.Liked)
            add += likedBonus;
        else if (result == NPCGiftPreference.GiftResult.Disliked)
            add += dislikedPenalty;
        else
            add += baseAffectionOnGift;

        if (affection != null)
            affection.AddAffection(add);
    }

    void PlayGiftDialogue(NPCGiftPreference.GiftResult result)
    {
        if (DialogueManager.Instance == null) return;
        if (affection == null) return;

        DialogueSequence target = null;

        switch (result)
        {
            case NPCGiftPreference.GiftResult.Loved:
                target = dialogueData.loveGiftSequence;
                break;
            case NPCGiftPreference.GiftResult.Liked:
                target = dialogueData.likeGiftSequence;
                break;
            case NPCGiftPreference.GiftResult.Disliked:
                target = dialogueData.dislikeGiftSequence;
                break;
            default:
                target = dialogueData.neutralGiftSequence;
                break;
        }

        if (target == null)
        {
            if (dialogueData.sequences != null && dialogueData.sequences.Length > 0)
            {
                target = dialogueData.sequences[0];
            }
        }

        if (target != null)
        {
            DialogueManager.Instance.StartDialogue(dialogueData, target);
        }
    }
}
