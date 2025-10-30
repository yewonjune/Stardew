using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopController : MonoBehaviour
{
    public GameObject shopPanel;            // ShopCanvas 안 Panel
    public Transform shopContent;           // 버튼들이 들어갈 Content
    public GameObject shopItemButtonPrefab;

    public PlayerWallet wallet;             // PlayerWallet.I 로도 가능
    public Text goldText;
    public Text hintText;                   // "슬롯을 클릭해서 넣어주세요" 안내
    public InventoryUILayout inventoryLayout;
    public InventoryUI inventoryUI;
    public Button cancelButton;

    [Header("Data")]
    public ShopCatalog catalog;

    // 내부 상태
    ShopItemEntry pendingBuy;                   // 구매 대기 중인 상품(아이템 1개)
    ShopItemButton pendingButton;          // 재고 갱신용
    int pendingCount = 0;

    List<ShopItemButton> spawned = new List<ShopItemButton>();

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
        if (data) catalog = data;
        BuildList();

        if (shopPanel) shopPanel.SetActive(true);
        if (inventoryUI) inventoryUI.Show(true);

        hintText.text = "상점에서 아이템을 고르거나, 인벤토리 아이템을 클릭해 판매하세요.";
        pendingBuy = null; pendingButton = null;
        // 인벤토리 슬롯 클릭 이벤트 연결
        inventoryUI.onSlotClicked -= OnInventorySlotClicked;
        inventoryUI.onSlotRightClicked -= OnInventorySlotRightClicked;
        inventoryUI.onSlotClicked += OnInventorySlotClicked;
        inventoryUI.onSlotRightClicked += OnInventorySlotRightClicked;

        inventoryUI.externalHandleEnabled = true;

        inventoryLayout?.ApplyShop();
    }

    public void CloseShop()
    {
        if (shopPanel) shopPanel.SetActive(false);
        if (inventoryUI) inventoryUI.Show(false);

        pendingBuy = null; 
        pendingButton = null;
        
        inventoryUI.onSlotClicked -= OnInventorySlotClicked;
        inventoryUI.onSlotRightClicked -= OnInventorySlotRightClicked;
        inventoryUI.externalHandleEnabled = false;

        inventoryLayout?.ApplyOriginal();
    }

    void BuildList()
    {
        // 기존 버튼 정리
        foreach (var b in spawned) Destroy(b.gameObject);
        spawned.Clear();

        foreach (var e in catalog.shopItemEntries)
        {
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
        if (goldText) goldText.text = $"{wallet.gold}g";
    }

    // ----- 구매 흐름 -----
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

        hintText.text = $"[{entry.item.itemName}] {pendingCount}개 구매 준비됨 " +
                             $"(예상 비용: {pendingCount * entry.buyPrice}g)\n" +
                                            "→ 넣을 인벤토리 슬롯을 클릭하세요.";
    }

    void OnInventorySlotClicked(int slotIndex)
    {
        // 판매 모드가 아니라면: 구매 배치
        if (pendingBuy != null)
        {
            TryPlacePurchasedItem(slotIndex);
            return;
        }

        // 구매 대기가 없으면: 판매 1개
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

        hintText.text = $"[{entry.item.itemName}] {canBuy}개 구매 완료! (-{totalCost}g)";
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

        var entry = catalog.shopItemEntries.Find(e => e.item == stack.item);
        int price = (entry != null) ? entry.sellPrice : 0;

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
        hintText.text = $"[{stack.item.itemName}] {amount}개 판매! (+{price * amount}g)";
    }
}
