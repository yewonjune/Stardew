using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SaveBuilder
{
    // === Build (런타임 -> DTO) =========================================
    public static SaveData Build(TimeManager tm, Transform player)
    {
        var data = new SaveData();

        // Meta
        data.meta = new MetaDTO
        {
            day = tm.day,
            hour = tm.hour,
            minute = tm.minute,
            lastScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            posX = player.position.x,
            posY = player.position.y,
            //money = 0 // 있으면 채우기
        };

        // Player (확장 여지)
        data.player = new PlayerDTO
        {
            //stamina = 0 // PlayerStaminaController 있으면 채우기
        };

        // Inventory
        var inv = Inventory.instance;
        int n = inv.items.Count;
        data.inventory = new InventoryDTO
        {
            slotCnt = inv.SlotCnt,
            slots = new ItemStackDTO[n]
        };
        for (int i = 0; i < n; i++)
        {
            var s = inv.items[i];
            data.inventory.slots[i] = new ItemStackDTO
            {
                itemId = (s != null && s.item != null && s.count > 0) ? s.item.name : null,
                count = (s != null) ? s.count : 0
            };
        }

        // World
        var wsm = WorldStateManager.Instance;
        var sceneList = new List<SceneEntryDTO>();
        foreach (var kv in wsm.SnapshotAllScenes())
        {
            string scene = kv.Key;
            var st = kv.Value;

            var dto = new SceneStateDTO
            {
                tilled = ToCells(st.tilled),
                watered = ToCells(st.watered),
                crops = ToCrops(st.crops),
                resources = ToResources(st.resources),
                initialSpawnDone = st.initialSpawnDone
            };
            sceneList.Add(new SceneEntryDTO { scene = scene, state = dto });
        }
        data.world = new WorldDTO { scenes = sceneList.ToArray() };

        return data;
    }

    static CellDTO[] ToCells(HashSet<Vector3Int> set)
    {
        var list = new List<CellDTO>(set.Count);
        foreach (var c in set) list.Add(new CellDTO { x = c.x, y = c.y });
        return list.ToArray();
    }

    static CropDTO[] ToCrops(Dictionary<Vector3Int, CropSave> dict)
    {
        var list = new List<CropDTO>(dict.Count);
        foreach (var kv in dict)
        {
            var c = kv.Key; var v = kv.Value;
            list.Add(new CropDTO
            {
                prefabId = v.prefabId,
                x = c.x,
                y = c.y,
                growthStage = v.growthStage,
                isWateredToday = v.isWateredToday,
                harvestedOnce = v.harvestedOnce
            });
        }
        return list.ToArray();
    }

    static ResourceDTO[] ToResources(List<ResourceSave> arr)
    {
        var list = new List<ResourceDTO>(arr.Count);
        foreach (var r in arr)
        {
            list.Add(new ResourceDTO
            {
                prefabId = r.prefabId,
                x = r.position.x,
                y = r.position.y,
                harvestedOrRemoved = r.harvestedOrRemoved
            });
        }
        return list.ToArray();
    }

    // === Apply (DTO -> 런타임) =========================================
    public static void Apply(SaveData data, TimeManager tm)
    {
        if (data == null) return;

        // 0) Null 가드
        if (data.meta == null) data.meta = new MetaDTO();
        if (data.inventory == null) data.inventory = new InventoryDTO { slotCnt = 0, slots = System.Array.Empty<ItemStackDTO>() };
        if (data.world == null) data.world = new WorldDTO { scenes = System.Array.Empty<SceneEntryDTO>() };

        // 시간/날짜
        if (tm != null)
        {
            tm.day = data.meta.day;
            tm.hour = data.meta.hour;
            tm.minute = data.meta.minute;
        }

        // 인벤토리
        var inv = Inventory.instance;
        if (inv != null)
        {
            inv.SlotCnt = data.inventory.slotCnt;
            var slots = data.inventory.slots ?? System.Array.Empty<ItemStackDTO>();

            for (int i = 0; i < inv.items.Count && i < slots.Length; i++)
            {
                var s = slots[i];
                if (s != null && !string.IsNullOrEmpty(s.itemId) && s.count > 0)
                {
                    // ★ 폴더 경로 확장: Seeds / Items.Resource / Items.Tools
                    var item =
                        Resources.Load<Item>($"Item/{s.itemId}") ??
                        Resources.Load<Item>($"Item/Seeds/{s.itemId}") ??
                        Resources.Load<Item>($"Item/Resource/{s.itemId}") ??
                        Resources.Load<Item>($"Item/Tools/{s.itemId}");

                    inv.items[i] = new ItemStack(item, s.count);
                }
                else inv.items[i] = new ItemStack(null, 0);
            }
            inv.ForceRefresh();
        }

        // 월드
        var wsm = WorldStateManager.Instance;
        if (wsm != null)
        {
            var map = new Dictionary<string, SceneState>();
            var scenes = data.world.scenes ?? System.Array.Empty<SceneEntryDTO>();

            foreach (var e in scenes)
            {
                if (e == null || string.IsNullOrEmpty(e.scene) || e.state == null) continue;

                var st = new SceneState();

                if (e.state.tilled != null)
                    foreach (var c in e.state.tilled) st.tilled.Add(new Vector3Int(c.x, c.y, 0));

                if (e.state.watered != null)
                    foreach (var c in e.state.watered) st.watered.Add(new Vector3Int(c.x, c.y, 0));

                if (e.state.crops != null)
                    foreach (var crop in e.state.crops)
                    {
                        if (crop == null) continue;
                        var cell = new Vector3Int(crop.x, crop.y, 0);
                        st.crops[cell] = new CropSave
                        {
                            prefabId = crop.prefabId,
                            cell = cell,
                            growthStage = crop.growthStage,
                            isWateredToday = crop.isWateredToday,
                            harvestedOnce = crop.harvestedOnce
                        };
                    }

                if (e.state.resources != null)
                    foreach (var r in e.state.resources)
                    {
                        if (r == null) continue;
                        st.resources.Add(new ResourceSave
                        {
                            prefabId = r.prefabId,
                            position = new Vector3(r.x, r.y, 0),
                            harvestedOrRemoved = r.harvestedOrRemoved
                        });
                    }

                st.initialSpawnDone = e.state.initialSpawnDone;
                map[e.scene] = st;
            }

            wsm.ReplaceAll(map);
        }
    }
}
