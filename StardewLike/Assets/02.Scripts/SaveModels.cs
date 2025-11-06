using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public MetaDTO meta;
    public PlayerDTO player;
    public InventoryDTO inventory;
    public WorldDTO world;
    public StoreDTO store;
}

[System.Serializable]
public class MetaDTO
{
    public int day, hour, minute;
    public string lastScene;
    public float posX, posY;
    public int gold;

    public string nickname;
    public string farmName;
}

[System.Serializable]
public class PlayerDTO
{
    // public int stamina;
}

// 인벤토리(인덱스 보존)
[System.Serializable]
public class InventoryDTO
{
    public int slotCnt;
    public ItemStackDTO[] slots;
}
[System.Serializable]
public class ItemStackDTO
{
    public string itemId;
    public int count;
}

// 월드(씬별 상태를 배열로 보관 - 딕셔너리 회피)
[System.Serializable]
public class WorldDTO
{
    public SceneEntryDTO[] scenes;
}

[System.Serializable]
public class SceneEntryDTO
{
    public string scene;
    public SceneStateDTO state;
}

[System.Serializable]
public class SceneStateDTO
{
    public CellDTO[] tilled;
    public CellDTO[] watered;
    public CropDTO[] crops;
    public ResourceDTO[] resources;
    public bool initialSpawnDone;
}

[System.Serializable] public class CellDTO { public int x, y; }

[System.Serializable]
public class CropDTO
{
    public string prefabId;
    public int x, y;
    public int growthStage;
    public bool isWateredToday;
    public bool harvestedOnce;
}

[System.Serializable]
public class ResourceDTO
{
    public string prefabId;
    public float x, y;
    public bool harvestedOrRemoved;
}

[System.Serializable]
public class StoreDTO
{
    public StoreItemDTO[] items;
}

[System.Serializable]
public class StoreItemDTO
{
    public string itemId;
    public int price;
    public int stock;
}