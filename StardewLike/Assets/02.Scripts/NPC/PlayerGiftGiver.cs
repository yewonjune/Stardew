using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGiftGiver : MonoBehaviour
{
    [Header("상호작용")]
    public float interactRange = 1.2f;
    public LayerMask npcLayer;

    [Header("참조")]
    public Inventory inventory;   // 네 인벤토리 관리하는 스크립트
    public HotbarManager hotbar;           // 현재 선택 슬롯 알려주는 스크립트

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryGiveGift();
        }
    }

    void TryGiveGift()
    {
        // 1. 앞에 NPC 있는지
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, interactRange, npcLayer);
        if (!hit) return;

        var receiver = hit.collider.GetComponent<NPCGiftReceiver>();
        if (!receiver) return;

        // 2. 내가 들고있는 아이템 (핫바 선택 슬롯)
        int selectedIndex = hotbar.GetSelectedIndex();
        var stack = inventory.GetStack(selectedIndex);
        if (stack == null || stack.IsEmpty) return;

        string itemId = stack.item.itemId;
        receiver.ReceiveGift(itemId);

        // 3. 인벤토리에서 진짜로 1개 빼기
        inventory.RemoveFromSlot(selectedIndex, 1);
    }
}
