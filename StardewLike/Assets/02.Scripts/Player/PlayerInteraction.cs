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
        if (!Input.GetKeyDown(interactKey)) return;

        if (DialogueManager.IsBusy && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnClickNext();
            return;
        }

        TryInteract();
    }

    void TryInteract()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange);

        foreach (Collider2D hit in hits)
        {
            if (TryGiftTo(hit))
            {
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
        var receiver = hit.GetComponent<NPCGiftReceiver>();
        if (receiver == null) return false;

        if (hotbar == null || Inventory.instance == null) return false;

        int slotIndex = hotbar.GetSelectedIndex();
        var stack = Inventory.instance.GetStack(slotIndex);

        if (stack == null || stack.IsEmpty) return false;

        if (stack.item != null && stack.item.canSell == false)
        {
            return false;
        }

        string itemId = stack.item.itemId;
        receiver.ReceiveGift(itemId);

        Inventory.instance.RemoveFromSlot(slotIndex, 1);

        return true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
