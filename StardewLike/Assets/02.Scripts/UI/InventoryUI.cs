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

    Item cursorItem;
    int cursorCount;
    public CursorItemUI cursorUI;

    void Start()
    {
        inventory = Inventory.instance;
        slots = slotHolder.GetComponentsInChildren<Slot>(includeInactive: true);

        inventory.onSlotCountChange += SlotChange;
        inventory.onInventoryChanged += Refresh;

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].SetIndex(i);
            slots[i].onClick = OnSlotClick;
        }

        InventoryPanel.SetActive(activeInventory);
        SlotChange(inventory.SlotCnt);
        Refresh();
        UpdateCursorUI();
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

            if (stack != null && stack.item != null && stack.count > 0)
                slots[i].SetItem(stack.item, stack.count);
            else
                slots[i].ClearSlot();
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

    void OnSlotClick(int slotIndex, UnityEngine.EventSystems.PointerEventData.InputButton btn, bool shift)
    {
        if (slotIndex < 0 || slotIndex >= inventory.items.Count) return;

        var slotStack = inventory.items[slotIndex] ?? new ItemStack(null, 0);
        var slotItem = slotStack.item;
        var slotCount = slotStack.count;

        // ---- 우클릭: 1개씩 이동 ----
        if (btn == UnityEngine.EventSystems.PointerEventData.InputButton.Right)
        {
            // 손이 비었고 슬롯에 있으면 1개만 집기
            if (cursorItem == null && slotItem != null && slotCount > 0)
            {
                cursorItem = slotItem;
                cursorCount = 1;
                slotStack.count -= 1;
                if (slotStack.count <= 0) inventory.items[slotIndex] = new ItemStack(null, 0);
                inventory.ForceRefresh();
                UpdateCursorUI();
                return;
            }

            // 손에 있고, 대상 슬롯이 비었으면 1개 내려놓기
            if (cursorItem != null && (slotItem == null || slotCount <= 0))
            {
                inventory.items[slotIndex] = new ItemStack(cursorItem, 1);
                cursorCount -= 1;
                if (cursorCount <= 0) { cursorItem = null; cursorCount = 0; }
                inventory.ForceRefresh();
                UpdateCursorUI();
                return;
            }

            // 손에 있고, 같은 아이템이면 1개 합치기(스택 가능 가정)
            if (cursorItem != null && slotItem == cursorItem && cursorItem.isStackable)
            {
                slotStack.count += 1;
                cursorCount -= 1;
                if (cursorCount <= 0) { cursorItem = null; cursorCount = 0; }
                inventory.ForceRefresh();
                UpdateCursorUI();
                return;
            }

            // 그 외: 아무 것도 안함(우클릭은 미세 조정용)
            return;
        }

        // ---- 좌클릭: 전량/절반 ----
        bool splitHalf = shift; // Shift+좌클릭 → 절반 나누기

        // 1) 손이 비었고 슬롯에 있으면 집기
        if (cursorItem == null)
        {
            if (slotItem == null || slotCount <= 0) return;

            if (splitHalf && slotCount > 1)
            {
                int half = slotCount / 2;
                cursorItem = slotItem;
                cursorCount = half;
                slotStack.count -= half;
                if (slotStack.count <= 0) inventory.items[slotIndex] = new ItemStack(null, 0);
            }
            else
            {
                cursorItem = slotItem;
                cursorCount = slotCount;
                inventory.items[slotIndex] = new ItemStack(null, 0);
            }

            inventory.ForceRefresh();
            UpdateCursorUI();
            return;
        }

        // 2) 손에 들고 있고, 대상 슬롯이 비었으면 내려놓기
        if (slotItem == null || slotCount <= 0)
        {
            int putCount = splitHalf ? Mathf.Max(1, cursorCount / 2) : cursorCount;
            inventory.items[slotIndex] = new ItemStack(cursorItem, putCount);
            cursorCount -= putCount;
            if (cursorCount <= 0) { cursorItem = null; cursorCount = 0; }
            inventory.ForceRefresh();
            UpdateCursorUI();
            return;
        }

        // 3) 손/슬롯 같은 아이템 & 스택 가능 → 합치기
        if (slotItem == cursorItem && cursorItem.isStackable)
        {
            int add = splitHalf ? Mathf.Max(1, cursorCount / 2) : cursorCount;
            slotStack.count += add;
            cursorCount -= add;
            if (cursorCount <= 0) { cursorItem = null; cursorCount = 0; }
            inventory.ForceRefresh();
            UpdateCursorUI();
            return;
        }

        // 4) 서로 다른 아이템 → 스왑(Shift 상관없이 전량 스왑)
        {
            var tempItem = slotItem;
            var tempCount = slotCount;
            inventory.items[slotIndex] = new ItemStack(cursorItem, cursorCount);
            cursorItem = tempItem;
            cursorCount = tempCount;

            if (cursorItem == null || cursorCount <= 0) { cursorItem = null; cursorCount = 0; }
            inventory.ForceRefresh();
            UpdateCursorUI();
        }
    }

    void UpdateCursorUI()
    {
        if (!cursorUI) return;
        cursorUI.Set(cursorItem, cursorCount);
    }

}
