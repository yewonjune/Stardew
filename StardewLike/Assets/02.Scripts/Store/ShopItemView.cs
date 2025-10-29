using System;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemView : MonoBehaviour
{
    [Header("UI")]
    public Image icon;
    public Text nameText;
    public Text priceText;
    public Text amountText;
    public Button minusBtn;
    public Button plusBtn;
    public Button confirmBtn; // Buy 또는 Sell

    Item boundItem;
    int unitPrice;
    int amount = 1;
    bool buying;
    Func<int> getMaxAmount; // 재고(구매) 또는 보유수(판매)
    Action<Item, int> onConfirm;

    public void Bind(Item item, int unitPrice, Func<int> getMaxAmount, bool isBuying, Action<Item, int> onConfirm)
    {
        boundItem = item;
        this.unitPrice = Mathf.Max(0, unitPrice);
        this.getMaxAmount = getMaxAmount;
        this.buying = isBuying;
        this.onConfirm = onConfirm;

        if (icon) icon.sprite = item.icon;
        if (nameText) nameText.text = item.itemName;
        Refresh();

        if (minusBtn) minusBtn.onClick.AddListener(() => ChangeAmount(-1));
        if (plusBtn) plusBtn.onClick.AddListener(() => ChangeAmount(+1));
        if (confirmBtn) confirmBtn.onClick.AddListener(() => onConfirm?.Invoke(boundItem, amount));
        if (confirmBtn) confirmBtn.GetComponentInChildren<Text>().text = isBuying ? "Buy" : "Sell";
    }

    void Refresh()
    {
        int max = Mathf.Max(1, getMaxAmount?.Invoke() ?? 1);
        amount = Mathf.Clamp(amount, 1, max);

        if (amountText) amountText.text = amount.ToString();
        if (priceText)
        {
            int total = unitPrice * amount;
            priceText.text = buying ? $"{total} G" : $"+{total} G";
        }
    }

    void ChangeAmount(int delta)
    {
        amount += delta;
        Refresh();
    }
}
