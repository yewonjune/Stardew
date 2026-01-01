using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopController : MonoBehaviour
{
    public GameObject shopPanel;
    public Transform shopContent;
    public GameObject shopItemButtonPrefab;

    public PlayerWallet wallet;
    public Text goldText;
    public Text hintText;
    public InventoryUILayout inventoryLayout;
    public InventoryUI inventoryUI;
    public Button cancelButton;

    [Header("Data")]
    public ShopCatalog catalog;

    // 내부 상태
    ShopItemEntry pendingBuy;
    ShopItemButton pendingButton;
    int pendingCount = 0;

    List<ShopItemButton> spawned = new List<ShopItemButton>();

    public bool IsOpen { get; private set; }

    void Awake()
    {
        if (!inventoryLayout)
            inventoryLayout = FindObjectOfType<InventoryUILayout>(true); 
        if (!inventoryUI)
            inventoryUI = FindObjectOfType<InventoryUI>(true);
    }

    void Start()
    {
        cancelButton.onClick.AddListener(() => CloseShop());
    }

    void OnEnable() { RefreshGold(); }

    public void OpenShop(ShopCatalog data = null)
    {
        if (IsOpen) return;
        IsOpen = true;

        if (data) catalog = data;
        BuildList();

        if (shopPanel) shopPanel.SetActive(true);

        if (inventoryUI) inventoryUI.Open();

        hintText.text = "상점에서 아이템을 고르거나, 인벤토리 아이템을 클릭해 판매하세요.";
        pendingBuy = null; pendingButton = null;

        inventoryUI.onSlotClicked -= OnInventorySlotClicked;
        inventoryUI.onSlotRightClicked -= OnInventorySlotRightClicked;
        inventoryUI.onSlotClicked += OnInventorySlotClicked;
        inventoryUI.onSlotRightClicked += OnInventorySlotRightClicked;

        inventoryUI.externalHandleEnabled = true;
        inventoryLayout?.ApplyShop();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseShop()
    {
        if (!IsOpen) return;
        IsOpen = false;

        if (shopPanel) shopPanel.SetActive(false);
        if (inventoryUI) inventoryUI.Show(false);

        pendingBuy = null; 
        pendingButton = null;
        
        inventoryUI.onSlotClicked -= OnInventorySlotClicked;
        inventoryUI.onSlotRightClicked -= OnInventorySlotRightClicked;
        inventoryUI.externalHandleEnabled = false;

        inventoryLayout?.ApplyOriginal();

        inventoryUI.HideCursorPreview();
        PlayerActionLock.Unlock("OpenShop");
    }

    void BuildList()
    {
        foreach (var b in spawned) Destroy(b.gameObject);
        spawned.Clear();

        Season currentSeason = SeasonManager.Instance != null
               ? SeasonManager.Instance.currentSeason
               : Season.Spring;

        foreach (var e in catalog.shopItemEntries)
        {
            // 현재 계절에 판매하지 않는 아이템이면 스킵
            if (!e.IsAvailableThisSeason(currentSeason))
                continue;

            var go = Instantiate(shopItemButtonPrefab, shopContent);
            var b = go.GetComponent<ShopItemButton>();
            b.Bind(e, this);
            spawned.Add(b);
        }

        RefreshGold();
    }

    void RefreshGold()
    {
        if (!wallet) wallet = PlayerWallet.Instance;
        if (goldText) goldText.text = $"{wallet.gold}골드";
    }

    public void BeginBuy(ShopItemEntry entry, ShopItemButton btn)
    {
        if (entry.stock == 0)
        {
            hintText.text = "재고가 없어요!";
            return;
        }
        if (!wallet.CanPay(entry.buyPrice))
        {
            hintText.text = "골드가 부족해요!";
            return;
        }

        if (pendingBuy == null || pendingBuy != entry)
        {
            pendingBuy = entry;
            pendingButton = btn;
            pendingCount = 0;
        }

        pendingCount++;

        int maxByStock = (entry.stock < 0) ? int.MaxValue : entry.stock;
        int maxByWallet = (wallet != null && entry.buyPrice > 0) ? (wallet.gold / entry.buyPrice) : int.MaxValue;
        int affordable = Mathf.Min(maxByStock, maxByWallet);

        int willBuy = Mathf.Min(pendingCount, affordable);

        if (willBuy <= 0)
        {
            if (wallet && wallet.gold < entry.buyPrice) hintText.text = "골드가 부족해요!";
            else hintText.text = "재고가 부족해요!";
            return;
        }

        if (inventoryUI) inventoryUI.ShowCursorPreview(entry.item, pendingCount);

        hintText.text = $"[{entry.item.itemName}] {pendingCount}개" +
                             $"({pendingCount * entry.buyPrice}골드)\n" +
                                            "→ 넣을 인벤토리 슬롯을 클릭하세요.";
    }

    void OnInventorySlotClicked(int slotIndex)
    {
        if (pendingBuy != null)
        {
            TryPlacePurchasedItem(slotIndex);
            return;
        }

        TrySellFromSlot(slotIndex, sellAll: false);
    }

    void OnInventorySlotRightClicked(int slotIndex)
    {
        // 우클릭: 전부 판매
        if (pendingBuy != null)
        {
            hintText.text = "구매 중에는 우클릭 판매가 안돼요. 먼저 슬롯에 넣어주세요.";
            return;
        }
        TrySellFromSlot(slotIndex, sellAll: true);
    }

    void TryPlacePurchasedItem(int slotIndex)
    {
        var entry = pendingBuy;
        
        if (entry == null || pendingCount <= 0)
        {
            hintText.text = "구매 대기 중인 아이템이 없어요.";
            return;
        }

        if (entry.item is BagItem bag)
        {
            bool handled = BagUpgradeService.TryApplyBagPurchase(
                entry, bag, wallet, inventoryUI, ref pendingCount,
                refreshGold: RefreshGold,
                onClearPending: () => { pendingBuy = null; pendingButton = null; pendingCount = 0; },
                setHint: (msg) => { hintText.text = msg; });

            if (handled)
            {
                pendingButton?.RefreshStock();
                return;
            }
        }

        int want = pendingCount;
        int maxByStock = (entry.stock < 0) ? int.MaxValue : entry.stock;
        int maxByWallet = (wallet != null && entry.buyPrice > 0) ? (wallet.gold / entry.buyPrice) : int.MaxValue;

        int canBuy = Mathf.Min(want, maxByStock, maxByWallet);
        if (canBuy <= 0)
        {
            hintText.text = (wallet && wallet.gold < entry.buyPrice) ? "골드가 부족해요!" : "재고가 부족해요!";
            return;
        }

        int totalCost = canBuy * entry.buyPrice;

        if (!wallet.TryPay(totalCost))
        {
            hintText.text = "골드가 부족해요!";
            return;
        }

        var inv = Inventory.instance;
        bool placed = inv.TryAddToSlot(slotIndex, entry.item, canBuy);

        if (!placed)
        {
            // 실패하면 돈 돌려주기
            wallet.Earn(totalCost);
            hintText.text = "그 슬롯에 들어가지 않아요. (빈 칸이거나 같은 아이템이어야 해요)";
            RefreshGold();
            return;
        }

        if (entry.stock > 0) entry.stock -= canBuy;
        pendingButton?.RefreshStock();

        hintText.text = $"[{entry.item.itemName}] {canBuy}개 구매 완료! (-{totalCost}골드)";
        RefreshGold();

        pendingCount -= canBuy;
        if (pendingCount <= 0)
        {
            pendingBuy = null;
            pendingButton = null;
            pendingCount = 0;
        }
        else
        {
            hintText.text += $"\n(잔여 대기: {pendingCount}개 → 다른 슬롯을 클릭해 넣을 수 있어요)";
        }

        if (pendingCount > 0) inventoryUI.ShowCursorPreview(pendingBuy.item, pendingCount);
        else inventoryUI.HideCursorPreview();
    }

    void TrySellFromSlot(int slotIndex, bool sellAll)
    {
        var inv = Inventory.instance;
        var stack = inv.GetStack(slotIndex);
        if (stack == null || stack.IsEmpty)
        {
            hintText.text = "빈 슬롯이에요.";
            return;
        }

        int price = (stack.item != null && stack.item.canSell) ? stack.item.sellPrice : 0;

        if (price <= 0)
        {
            hintText.text = "이 아이템은 판매할 수 없어요.";
            return;
        }

        int amount = sellAll ? stack.count : 1;
        if (amount <= 0) 
        { 
            hintText.text = "팔게 없어요.";
            return;
        }

        inv.RemoveFromSlot(slotIndex, amount);

        wallet.Earn(price * amount);
        RefreshGold();
        hintText.text = $"[{stack.item.itemName}] {amount}개 판매! (+{price * amount}골드)";
    }

    void OnDisable()
    {
        if (IsOpen) CloseShop();
        else PlayerActionLock.Unlock("OpenShop");
    }
}
