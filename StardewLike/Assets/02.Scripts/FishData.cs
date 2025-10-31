using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFishData", menuName = "Fishing/Fish Data")]
public class FishData : ScriptableObject
{
    [Header("인벤토리 아이템 참조 (Item SO)")]
    public Item item;

    [Header("낚시 확률 가중치 (높을수록 잘 잡힘)")]
    [Range(0f, 1f)] public float weight = 0.25f;

    [Header("리액션 보너스 (반응시간 짧을수록 보너스 확률)")]
    [Range(0f, 1f)] public float reactionBonus = 0f;

    [Header("판매 가격 및 크기 정보")]
    public int basePrice = 10;
    public Vector2Int sizeRange = new Vector2Int(10, 50); // cm 단위

}
