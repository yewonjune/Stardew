using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    Inventory inventory;

    public GameObject InventoryPanel;
    public CursorItemUI cursorUI;
    bool activeInventory = false;

    public Slot[] slots;
    public Transform slotHolder;

    Item cursorItem;
    int cursorCount;
    
    public bool externalHandleEnabled = false;

    public Action<int> onSlotClicked;
    public Action<int> onSlotRightClicked;

    void Start()
    {
        inventory = Inventory.instance;
        slots = slotHolder.GetComponentsInChildren<Slot>(includeInactive: true);

        inventory.onSlotCountChange += SlotChange;
        inventory.onInventoryChanged += Refresh;

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].SetIndex(i);
            //slots[i].onClick = OnSlotClick;

            slots[i].onClick = (si, btn, shift) => HandleSlotClick(si, btn, shift);

        }

        InventoryPanel.SetActive(activeInventory);
        SlotChange(inventory.SlotCnt);
        Refresh();
        UpdateCursorUI();
    }

    void HandleSlotClick(int slotIndex, UnityEngine.EventSystems.PointerEventData.InputButton btn, bool shift)
    {
        if (externalHandleEnabled)
        {
            if (btn == UnityEngine.EventSystems.PointerEventData.InputButton.Left)
                onSlotClicked?.Invoke(slotIndex);
            else if (btn == UnityEngine.EventSystems.PointerEventData.InputButton.Right)
                onSlotRightClicked?.Invoke(slotIndex);
            return; // 내부 집기/스왑 로직 실행하지 않음
        }

        OnSlotClick(slotIndex, btn, shift);
    }


    void SlotChange(int val)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            bool enabled = (i < inventory.SlotCnt);
            var btn = slots[i].GetComponent<Button>();
            if (btn) btn.interactable = enabled;

            if (!enabled)
                slots[i].ClearSlot();
        }
    }

    void Refresh()
    {
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
            if (activeInventory) Close();
            else Open();
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

        if (btn == UnityEngine.EventSystems.PointerEventData.InputButton.Right)
        {
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

            return;
        }

        // ---- 좌클릭: 전량/절반 ----
        bool splitHalf = shift; // Shift+좌클릭 → 절반 나누기

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
    public void Show(bool on)
    {
        if (on) Open();
        else Close();
    }
    public void Open()
    {
        activeInventory = true;
        if (InventoryPanel) InventoryPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (cursorUI)
        {
            cursorUI.gameObject.SetActive(true);
            cursorUI.transform.SetAsLastSibling();
        }

        Refresh();
        UpdateCursorUI();
    }

    public void Close()
    {
        activeInventory = false;
        if (InventoryPanel) InventoryPanel.SetActive(false);

        if (cursorUI) cursorUI.gameObject.SetActive(false);
    }
    public void ShowCursorPreview(Item item, int count)
    {
        cursorItem = item;
        cursorCount = count;

        if (cursorUI)
        {
            cursorUI.gameObject.SetActive(true);
            cursorUI.transform.SetAsLastSibling();
            cursorUI.Set(item, count);
        }
    }

    public void HideCursorPreview()
    {
        cursorItem = null; cursorCount = 0;
        if (cursorUI)
        {
            cursorUI.Set(null, 0);
            cursorUI.gameObject.SetActive(false);
        }
    }
}
