using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] float interactRange = 1.5f; // 상호작용 거리
    [SerializeField] KeyCode interactKey = KeyCode.E;

    public HotbarManager hotbar;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        // 플레이어 주변의 콜라이더 감지
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange);

        foreach (Collider2D hit in hits)
        {
            if (TryGiftTo(hit))
            {
                // 선물 성공했으면 여기서 끝. 더 이상 평소 대화 안 함.
                return;
            }

            NPCMovement npc = hit.GetComponent<NPCMovement>();
            if (npc != null)
            {
                npc.Interact();
                break;
            }
        }
    }

    bool TryGiftTo(Collider2D hit)
    {
        // NPC가 선물 받을 수 있는지
        var receiver = hit.GetComponent<NPCGiftReceiver>();
        if (receiver == null) return false;

        if (hotbar == null || Inventory.instance == null) return false;

        // 현재 선택된 슬롯
        int slotIndex = hotbar.GetSelectedIndex();
        var stack = Inventory.instance.GetStack(slotIndex);

        // 손에 아무것도 없으면 선물 X
        if (stack == null || stack.IsEmpty) return false;

        if (stack.item != null && stack.item.canSell == false)
        {
            return false;  // 그냥 대화로 넘어감
        }

        // 아이템에 id가 있다고 가정
        string itemId = stack.item.itemId;
        receiver.ReceiveGift(itemId);

        // 인벤토리에서 1개 빼기
        Inventory.instance.RemoveFromSlot(slotIndex, 1);

        return true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
