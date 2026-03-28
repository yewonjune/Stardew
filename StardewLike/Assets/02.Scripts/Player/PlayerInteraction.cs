using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] float interactRange = 1.5f;
    [SerializeField] KeyCode interactKey = KeyCode.E;
    [SerializeField] float promptOffsetY = 0.3f;     // 머리 위 오프셋

    public HotbarManager hotbar;
    public InteractPromptUI promptUI;                // 인스펙터에서 연결

    IInteractable _current;   // 현재 범위 안의 대상

    void Update()
    {
        DetectNearest();

        if (!Input.GetKeyDown(interactKey)) return;

        // 다이얼로그 진행 중이면 Next
        if (DialogueManager.IsBusy && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnClickNext();
            return;
        }

        TryInteract();
    }

    void DetectNearest()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange);

        IInteractable nearest = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            // 선물 우선 처리 대상 체크 (기존 로직 유지)
            var receiver = hit.GetComponent<NPCGiftReceiver>();
            if (receiver != null && CanGift())
            {
                nearest = receiver.GetComponent<IInteractable>();
                if (nearest != null) { minDist = 0f; break; }
            }

            var interactable = hit.GetComponent<IInteractable>();
            if (interactable == null) continue;

            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = interactable;
            }
        }

        // 대상 바뀌었을 때만 UI 갱신
        if (nearest != _current)
        {
            _current = nearest;

            if (_current != null)
            {
                var targetPos = (_current as MonoBehaviour)?.transform.position ?? transform.position;
                promptUI?.Show(targetPos + Vector3.up * promptOffsetY, _current.InteractLabel);
            }
            else
            {
                promptUI?.Hide();
            }
        }
        // 대상은 같지만 위치가 움직일 수 있으니 매 프레임 위치 갱신
        else if (_current != null)
        {
            var targetPos = (_current as MonoBehaviour)?.transform.position ?? transform.position;
            promptUI?.Show(targetPos + Vector3.up * promptOffsetY, _current.InteractLabel);
        }
    }

    void TryInteract()
    {
        if (_current == null) return;

        // 선물 먼저 시도
        var col = (_current as MonoBehaviour)?.GetComponent<Collider2D>();
        if (col != null && TryGiftTo(col)) return;

        _current.Interact();
    }

    bool CanGift()
    {
        if (hotbar == null || Inventory.instance == null) return false;
        var stack = Inventory.instance.GetStack(hotbar.GetSelectedIndex());
        return stack != null && !stack.IsEmpty && stack.item != null && stack.item.canSell;
    }

    bool TryGiftTo(Collider2D hit)
    {
        var receiver = hit.GetComponent<NPCGiftReceiver>();
        if (receiver == null) return false;
        if (!CanGift()) return false;

        int slotIndex = hotbar.GetSelectedIndex();
        var stack = Inventory.instance.GetStack(slotIndex);

        receiver.ReceiveGift(stack.item.itemId);
        Inventory.instance.RemoveFromSlot(slotIndex, 1);
        return true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}