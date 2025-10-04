using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    public Image icon;
    public Text countText;

    Item itemData;
    int count;

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
}
