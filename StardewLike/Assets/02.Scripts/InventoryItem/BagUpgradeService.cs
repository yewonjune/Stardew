using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BagUpgradeService : MonoBehaviour
{
    public static bool TryApplyBagPurchase(
       ShopItemEntry entry,
       BagItem bag,
       PlayerWallet wallet,
       InventoryUI inventoryUI,
       ref int pendingCount,
       System.Action refreshGold,
       System.Action onClearPending,
       System.Action<string> setHint)
    {
        int cur = Inventory.instance.SlotCnt;
        int maxUISlots = (inventoryUI != null && inventoryUI.slots != null) ? inventoryUI.slots.Length : 999;

        int incPerBag = Mathf.Max(1, bag.slotIncrease);
        int maxBagsByUISpace = Mathf.Max(0, (maxUISlots - cur) / incPerBag);
        if (maxBagsByUISpace <= 0)
        {
            setHint?.Invoke("더 이상 인벤토리를 확장할 수 없어요.");
            inventoryUI?.HideCursorPreview();
            onClearPending?.Invoke();
            return true;
        }

        int maxByStock = (entry.stock < 0) ? int.MaxValue : entry.stock;
        int maxByWallet = (wallet != null && entry.buyPrice > 0) ? (wallet.gold / entry.buyPrice) : int.MaxValue;

        int wantBags = pendingCount;
        int canBuyBags = Mathf.Min(wantBags, maxBagsByUISpace, maxByStock, maxByWallet);
        if (canBuyBags <= 0)
        {
            setHint?.Invoke((wallet && entry.buyPrice > wallet.gold) ? "골드가 부족해요!" : "재고가 부족해요!");
            return true;
        }

        int totalCost = canBuyBags * entry.buyPrice;
        if (!wallet.TryPay(totalCost))
        {
            setHint?.Invoke("골드가 부족해요!");
            return true;
        }

        int totalInc = canBuyBags * incPerBag;
        int target = Mathf.Min(cur + totalInc, maxUISlots);
        Inventory.instance.SlotCnt = target;

        if (entry.stock > 0) entry.stock -= canBuyBags;

        refreshGold?.Invoke();

        setHint?.Invoke($"가방 {canBuyBags}개 구매 완료! 인벤토리 {cur}칸 → {target}칸 (-{totalCost}g)");

        pendingCount -= canBuyBags;
        if (pendingCount <= 0)
        {
            onClearPending?.Invoke();
            inventoryUI?.HideCursorPreview();
        }
        else
        {
            inventoryUI?.ShowCursorPreview(entry.item, pendingCount);
            setHint?.Invoke($"가방 {canBuyBags}개 구매 완료! 인벤토리 {cur}칸 → {target}칸 (-{totalCost}g)\n(잔여 대기: {pendingCount}개 → 계속 확장 가능하면 적용돼요)");
        }

        return true;
    }
}
