using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemStack
{
    public Item item;
    public int count;

    public ItemStack(Item item, int count = 1)
    {
        this.item = item;
        this.count = count;
    }
}

public class Inventory : MonoBehaviour
{
    #region Singleton
    public static Inventory instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(instance);
        }

        instance = this;

        if (SlotCnt <= 0) SlotCnt = 10;
    }
    #endregion

    public delegate void OnSlotCountChange(int val);
    public OnSlotCountChange onSlotCountChange;
    
    public event Action onInventoryChanged;

    private int slotCnt;

    public int SlotCnt
    {
        get => slotCnt;
        set
        {
            slotCnt = value;
            onSlotCountChange?.Invoke(slotCnt);
            onInventoryChanged?.Invoke();
        }
    }

    public List<ItemStack> items = new List<ItemStack>();

    public bool AddItem(Item item, int amount = 1)
    {
        if (item == null || amount <= 0)
            return false;

        // 1) 스택 가능한 아이템이면 기존 스택을 먼저 채움
        if (item.isStackable)
        {
            ItemStack stack = items.Find(s => s.item == item);
            if (stack != null)
            {
                stack.count += amount;
                Debug.Log($"[Inventory] '{item.itemName}' 스택 +{amount} (총 {stack.count})");
                onInventoryChanged?.Invoke();
                return true;
            }
        }

        // 2) 남는 슬롯이 있어야 새 스택/아이템 추가 가능
        if (items.Count >= SlotCnt)
        {
            Debug.Log("[Inventory] 인벤토리가 가득 참!");
            return false;
        }

        items.Add(new ItemStack(item, amount));
        Debug.Log($"[Inventory] '{item.itemName}' 추가됨 (x{amount})");

        onInventoryChanged?.Invoke();
        return true;
    }

    public bool RemoveItem(Item item, int amount = 1)
    {
        if (item == null || amount <= 0)
            return false;

        bool anyAdded = false;
        int remaining = amount;

        ItemStack stack = items.Find(s => s.item == item);
        if (stack == null)
            return false;

        stack.count -= amount;
        if (stack.count <= 0)
            items.Remove(stack);

        onInventoryChanged?.Invoke();
        return true;
    }


    public void ForceRefresh()
    {
        onInventoryChanged?.Invoke();
    }
}
