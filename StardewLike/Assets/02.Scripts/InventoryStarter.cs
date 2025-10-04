using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryStarter : MonoBehaviour
{
    public Item[] starterItems;
    public bool onlyFillEmpty = true;

    // Start is called before the first frame update
    void Start()
    {
        var inv = Inventory.instance;
        if (inv == null) return;

        int limit = Mathf.Min(10, inv.SlotCnt, starterItems != null ? starterItems.Length : 0);

        for (int i = 0; i < limit; i++)
        {
            Item it = starterItems[i];
            if (it == null) continue;

            // 이미 i번째에 뭔가 있다면 건너뛰거나(onlyFillEmpty),
            // 비워 넣고 싶으면 아래 주석 해제해서 강제 배치 로직을 만들 수도 있음.
            if (onlyFillEmpty)
            {
                if (i < inv.items.Count && inv.items[i] != null && inv.items[i].item != null)
                    continue; // 건너뛰기
            }

            // i번째 위치에 정확히 넣고 싶다면:
            // - 리스트 길이를 i+1까지 채우고
            while (inv.items.Count <= i) inv.items.Add(new ItemStack(null, 0));
            inv.items[i] = new ItemStack(it, 1);
        }

        inv.ForceRefresh();
    }

}
