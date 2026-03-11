using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Inventory;

public class HotbarManager : MonoBehaviour
{
    public HotbarSlotUI[] slots;

    private int selectedIndex = 0;

    Inventory inventory;

    // Start is called before the first frame update
    void Start()
    {
        inventory = Inventory.instance;

        if (inventory != null)
        {
            inventory.onInventoryChanged += RefreshFromInventory;
            inventory.onSlotCountChange += OnSlotCountChanged;
        }

        RefreshFromInventory();
        HighlightSlot(selectedIndex);
    }

    void OnDestroy()
    {
        // 구독 해제 (플레이 모드 종료/씬 전환 시 안전)
        if (inventory != null)
        {
            inventory.onInventoryChanged -= RefreshFromInventory;
            inventory.onSlotCountChange -= OnSlotCountChanged;
        }
    }

    // Update is called once per frame
    void Update()
    {
        int maxHotbar = Mathf.Min(10, slots.Length);

        for (int i = 0; i < maxHotbar; i++)
        {
            if(Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
            }
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if(Mathf.Abs(scroll) > 0.0001f)
        {
            int direction = scroll > 0 ? -1 : 1;
            int newIndex = (selectedIndex + direction + maxHotbar) % maxHotbar;
            SelectSlot(newIndex);
        }
    }

    void RefreshFromInventory()
    {
        int maxHotbar = Mathf.Min(10, slots.Length);
        if (inventory == null)
        {
            for (int i = 0; i < maxHotbar; i++) slots[i].ClearSlot();
            return;
        }

        for (int i = 0; i < maxHotbar; i++)
        {
            bool withinInvSlots = (i < inventory.SlotCnt);
            bool hasItem = (i < inventory.items.Count);

            if (withinInvSlots && hasItem && inventory.items[i] != null && inventory.items[i].item != null)
            {
                var st = inventory.items[i];
                slots[i].SetItem(st.item, st.count);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }

        // 선택 하이라이트 범위 보정
        selectedIndex = Mathf.Clamp(selectedIndex, 0, maxHotbar - 1);
        HighlightSlot(selectedIndex);
    }

    void OnSlotCountChanged(int _) => RefreshFromInventory();

    void SelectSlot(int index)
    {
        if (index == selectedIndex)
            return;

        selectedIndex = index;
        HighlightSlot(index);
    }

    void HighlightSlot(int index)
    {
        int maxHotbar = Mathf.Min(10, slots.Length);

        for (int i = 0; i < maxHotbar; i++)
        {
            slots[i].SetHighlight(i == index);
        }
    }

    public Item GetSelectedItem()
    {
        return slots[selectedIndex].GetItem();
    }

    public int GetSelectedIndex()
    {
        return selectedIndex;
    }


}
