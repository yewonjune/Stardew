using System.Collections;
using System.Collections.Generic;
using UnityEditor.Sprites;
using UnityEngine;

public class NPCGiftReceiver : MonoBehaviour
{
    public NPCGiftPreference giftPreference;
    public DialogueData dialogueData;
    public int baseAffectionOnGift = 5;  // 기본 선물 호감도
    public int lovedBonus = 15;
    public int likedBonus = 8;
    public int dislikedPenalty = -5;

    NPCAffection affection;

    void Awake()
    {
        affection = GetComponent<NPCAffection>();
    }

    public void ReceiveGift(string itemId)
    {
        int add = baseAffectionOnGift;

        var result = NPCGiftPreference.GiftResult.Neutral;
        if (giftPreference)
            result = giftPreference.Evaluate(itemId);

        // 선물 반응 대사 보여주기
        PlayGiftDialogue(result);

        // 호감도 반영
        if (result == NPCGiftPreference.GiftResult.Loved)
            add += lovedBonus;
        else if (result == NPCGiftPreference.GiftResult.Liked)
            add += likedBonus;
        else if (result == NPCGiftPreference.GiftResult.Disliked)
            add += dislikedPenalty;

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
            // 일반 대사 배열이 있고 1개 이상이면 그중 첫 번째 사용
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
