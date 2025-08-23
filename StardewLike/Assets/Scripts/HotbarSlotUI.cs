using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotbarSlotUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI countText;
    public Image highlightBorder;

    private Item currentItem;
    private int count;

    public void SetItem(Item item, int count)
    {
        currentItem = item;
        this.count = count;

        iconImage.sprite = item.icon;
        iconImage.enabled = true;
        countText.text = count.ToString();
    }

    public void ClearSlot()
    {
        currentItem = null;
        iconImage.sprite=null;
        iconImage.enabled = false;
        countText.text = "";
    }

    public void SetHighlight(bool isHighlighted)
    {
        if(highlightBorder != null)
        {
            highlightBorder.enabled = isHighlighted;
        }
    }

}
