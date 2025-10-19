using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CursorItemUI : MonoBehaviour
{
    public Image icon;
    public Text countText;
    Vector3 offset = new Vector3(12f, -12f, 0f);
    void Awake() { SetVisible(false); }

    public void SetVisible(bool v)
    {
        if (icon) icon.enabled = v;
        if (countText) countText.enabled = v;
    }
    public void Set(Item item, int count)
    {
        if (!item)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);
        if (icon) icon.sprite = item.icon;
        if (countText) countText.text = count > 1 ? count.ToString() : "";
    }

    void LateUpdate()
    {
        var pos = Input.mousePosition + offset;
        transform.position = pos;
    }
}
