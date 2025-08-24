using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HotbarManager : MonoBehaviour
{
    public HotbarSlotUI[] slots;
    public Item[] startItems;

    private int selectedIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        for(int i=0; i<slots.Length; i++)
        {
            if(i< startItems.Length && startItems[i] != null)
            {
                slots[i].SetItem(startItems[i], 1);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }

        HighlightSlot(selectedIndex);
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < Mathf.Min(9, slots.Length); i++)
        {
            if(Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
            }
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if(scroll != 0)
        {
            int direction = scroll > 0 ? 1 : -1;
            int newIndex = (selectedIndex + direction + slots.Length) % slots.Length;
            SelectSlot(newIndex);
        }
    }

    void SelectSlot(int index)
    {
        if (index == selectedIndex)
            return;

        selectedIndex = index;
        HighlightSlot(index);
        Debug.Log($"슬롯 {index} 선택됨. 아이템: {slots[index].GetItem()?.itemName ?? "없음"}");
    }

    void HighlightSlot(int index)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].SetHighlight(i == index);
        }
    }

    public Item GetSelectedItem()
    {
        Item selected = slots[selectedIndex].GetItem();
        Debug.Log($"현재 선택된 슬롯: {selectedIndex}, 아이템: {selected?.itemName ?? "없음"}");
        return selected;
    }
}
