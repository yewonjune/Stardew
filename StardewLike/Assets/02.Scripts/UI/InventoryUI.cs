using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    Inventory inventory;

    public GameObject InventoryPanel;
    bool activeInventory = false;

    public Slot[] slots;
    public Transform slotHolder;


    void Start()
    {
        inventory = Inventory.instance;
        slots = slotHolder.GetComponentsInChildren<Slot>(includeInactive: true);

        inventory.onSlotCountChange += SlotChange;
        inventory.onInventoryChanged += Refresh;

        InventoryPanel.SetActive(activeInventory);
        SlotChange(inventory.SlotCnt);
        Refresh();
    }

    void SlotChange(int val)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            bool enabled = (i < inventory.SlotCnt);
            var btn = slots[i].GetComponent<Button>();
            if (btn) btn.interactable = enabled;

            // 슬롯 수보다 큰 인덱스는 비워놓기(보여도 클릭만 막힘. 필요하면 SetActive도 가능)
            if (!enabled)
                slots[i].ClearSlot();
        }
    }

    // 인벤토리 내용물 → 슬롯 UI 반영
    void Refresh()
    {
        // 모든 슬롯 초기화
        for (int i = 0; i < slots.Length; i++)
            slots[i].ClearSlot();

        // items를 앞에서부터 슬롯에 채움
        for (int i = 0; i < inventory.items.Count && i < inventory.SlotCnt && i < slots.Length; i++)
        {
            var stack = inventory.items[i];
            slots[i].SetItem(stack.item, stack.count);
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            activeInventory = !activeInventory;
            InventoryPanel.SetActive(activeInventory);
        }
        
    }

    public void AddSlot()
    {
        inventory.SlotCnt += 10;
    }

}
