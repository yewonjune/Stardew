using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    public Text countText;

    public int slotIndex = -1;
    Item itemData;
    int count;

    public System.Action<int, PointerEventData.InputButton, bool> onClick;

    public void SetIndex(int index) => slotIndex = index;

    public void SetItem(Item newItem, int newCount = 1)
    {
        itemData = newItem;
        count = newCount;

        if (itemData != null)
        {
            icon.sprite = itemData.icon;
            icon.enabled = true;
            countText.text = count > 1 ? count.ToString() : "";
        }
        else
        {
            ClearSlot();
        }
    }
    public void ClearSlot()
    {
        itemData = null;
        count = 0;
        icon.sprite = null;
        icon.enabled = false;
        countText.text = "";
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke(slotIndex, eventData.button, Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
    }
}
