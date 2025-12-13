using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishCatchInventoryBinder : MonoBehaviour
{
    [Header("Auto Wiring")]
    [SerializeField] PlayerFishingController fishing; // 비워두면 자동 탐색

    Inventory inventory;
    InventoryUI inventoryUI;

    [Header("UX Options")]
    [Tooltip("잡자마자 인벤토리를 열어 보여줄지")]
    public bool openInventoryOnCatch = false;

    [Tooltip("가득 차서 못 넣을 때, 커서 프리뷰로 보여줄지")]
    public bool previewCursorOnCatch = true;

    void Awake()
    {
        if (!fishing)
            fishing = GetComponent<PlayerFishingController>() ?? FindObjectOfType<PlayerFishingController>(true);

        inventory = Inventory.instance ?? FindObjectOfType<Inventory>(true);
        inventoryUI = FindObjectOfType<InventoryUI>(true);

        if (!fishing) Debug.LogError("[FishCatchInventoryBinder] PlayerFishingController를 찾지 못했습니다.");
        if (!inventory) Debug.LogError("[FishCatchInventoryBinder] Inventory.instance를 찾지 못했습니다.");
    }

    void OnEnable()
    {
        if (fishing != null)
            fishing.OnFishCaught.AddListener(OnFishCaught);
    }

    void OnDisable()
    {
        if (fishing != null)
            fishing.OnFishCaught.RemoveListener(OnFishCaught);
    }

    void OnFishCaught(Item item, int count)
    {
        if (inventory == null || item == null || count <= 0) return;

        bool ok = inventory.AddItem(item, count);
        if (!ok)
        {
            Debug.Log("[FishCatchInventoryBinder] 인벤토리가 가득 차서 추가하지 못했습니다.");

            return;
        }

        if (openInventoryOnCatch && inventoryUI)
            inventoryUI.Show(true); // 인벤토리 열어주기(선택)
    }
}
