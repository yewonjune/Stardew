using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopItemButton : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    public Text nameText;
    public Text priceText;
    public Text stockText;
    //public Button buyButton;

    ShopItemEntry bound;
    ShopController controller;

    // Start is called before the first frame update
    public void Bind(ShopItemEntry e, ShopController c)
    {
        bound = e; controller = c;
        if (icon) icon.sprite = e.item.icon;
        if (nameText) nameText.text = e.item.itemName;
        if (priceText) priceText.text = $"{e.buyPrice}g";
        if (stockText)
        {
            stockText.gameObject.SetActive(e.stock >= 0);
            if (e.stock >= 0) stockText.text = $"x{e.stock}";
        }
        // buyButton 관련 코드 전부 제거
    }

    // 셀 아무 곳이나 좌클릭하면 구매 시작
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        controller.BeginBuy(bound, this);
    }

    public void RefreshStock()
    {
        if (!stockText) return;
        stockText.gameObject.SetActive(bound.stock >= 0);
        if (bound.stock >= 0) stockText.text = $"x{bound.stock}";
    }
}
