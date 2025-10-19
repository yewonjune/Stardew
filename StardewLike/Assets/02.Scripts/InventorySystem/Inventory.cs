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

    public bool IsEmpty => item == null || count <= 0;
}

public class Inventory : MonoBehaviour
{
    #region Singleton
    public static Inventory instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }

        instance = this;

        if (SlotCnt <= 0) SlotCnt = 10;

        EnsureSize(SlotCnt);
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
            EnsureSize(slotCnt);
            onSlotCountChange?.Invoke(slotCnt);
            onInventoryChanged?.Invoke();
        }
    }

    public List<ItemStack> items = new List<ItemStack>();

    void EnsureSize(int n)
    {
        while (items.Count < n) items.Add(new ItemStack(null, 0));
        if (items.Count > n) items.RemoveRange(n, items.Count - n);
    }

    int FindEmptyIndex()
    {
        for (int i = 0; i < SlotCnt && i < items.Count; i++)
            if (items[i] == null || items[i].IsEmpty) return i;
        return -1;
    }

    bool HasEmptySlot() => FindEmptyIndex() >= 0;

    public bool AddItem(Item item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;

        if (item.isStackable)
        {
            for (int i = 0; i < SlotCnt && i < items.Count; i++)
            {
                var s = items[i];
                if (!s.IsEmpty && s.item == item)
                {
                    s.count += amount;
                    onInventoryChanged?.Invoke();
                    return true;
                }
            }
        }

        int idx = FindEmptyIndex();
        if (idx == -1)
        {
            Debug.Log("[Inventory] 인벤토리가 가득 참!");
            return false;
        }

        items[idx] = new ItemStack(item, amount);
        onInventoryChanged?.Invoke();
        return true;
    }

    public bool RemoveItem(Item item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;

        for (int i = 0; i < SlotCnt && i < items.Count; i++)
        {
            var s = items[i];
            if (!s.IsEmpty && s.item == item)
            {
                s.count -= amount;
                if (s.count <= 0)
                {
                    items[i] = new ItemStack(null, 0);
                }
                onInventoryChanged?.Invoke();
                return true;
            }
        }
        return false;
    }

    public void ForceRefresh()
    {
        onInventoryChanged?.Invoke();
    }
}
