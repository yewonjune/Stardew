using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class CropSave
{
    public string prefabId;          // 어떤 작물인지(프리팹 이름/ID)
    public Vector3Int cell;          // 그리드 좌표
    public int growthStage;          // 성장 단계
    public bool isWateredToday;      // 필요 시
    public bool harvestedOnce;        // 재성장 여부 판단에 필요 (복원 정확도 향상)
}

[System.Serializable]
public class ResourceSave
{
    public string prefabId;          // rock, stump 등
    public Vector3 position;         // 월드 좌표
    public bool harvestedOrRemoved;  // 캐졌으면 true
}

[System.Serializable]
public class SceneState
{
    public HashSet<Vector3Int> tilled = new();      // 갈아둔 땅
    public HashSet<Vector3Int> watered = new();     // 물 준 타일
    public Dictionary<Vector3Int, CropSave> crops = new(); // 심은 작물
    public List<ResourceSave> resources = new();    // 바위/그루터기 등
    public bool initialSpawnDone;                   // 랜덤 스폰 1회 제한 플래그
}

public class WorldStateManager : MonoBehaviour
{
    public static WorldStateManager Instance { get; private set; }

    private readonly Dictionary<string, SceneState> _scenes = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    public IEnumerable<KeyValuePair<string, SceneState>> SnapshotAllScenes()
        => _scenes;

    public void ReplaceSceneState(string sceneName, SceneState newState)
    {
        _scenes[sceneName] = newState ?? new SceneState();
    }

    public void ReplaceAll(Dictionary<string, SceneState> all)
    {
        _scenes.Clear();
        if (all == null) return;
        foreach (var kv in all)
            _scenes[kv.Key] = kv.Value;
    }

    public SceneState GetOrCreate(string sceneName)
    {
        if (!_scenes.TryGetValue(sceneName, out var st))
        {
            st = new SceneState();
            _scenes[sceneName] = st;
        }
        return st;
    }

    public void MarkInitialSpawnDone(string sceneName)
    {
        GetOrCreate(sceneName).initialSpawnDone = true;
    }

    // Soil
    public void SetTilled(string sceneName, Vector3Int cell, bool value)
    {
        var s = GetOrCreate(sceneName);
        if (value) s.tilled.Add(cell); else s.tilled.Remove(cell);
    }
    public void SetWatered(string sceneName, Vector3Int cell, bool value)
    {
        var s = GetOrCreate(sceneName);
        if (value) s.watered.Add(cell); else s.watered.Remove(cell);
    }

    // Crops
    public void UpsertCrop(string sceneName, CropSave save)
    {
        var s = GetOrCreate(sceneName);
        s.crops[save.cell] = save;
    }
    public void RemoveCrop(string sceneName, Vector3Int cell)
    {
        var s = GetOrCreate(sceneName);
        s.crops.Remove(cell);
    }

    // Resources
    public void AddResource(string sceneName, ResourceSave save)
    {
        var s = GetOrCreate(sceneName);
        s.resources.Add(save);
    }
    public void MarkResourceRemoved(string sceneName, Vector3 position)
    {
        var s = GetOrCreate(sceneName);
        var idx = s.resources.FindIndex(r => (r.position - position).sqrMagnitude < 0.0001f);
        if (idx >= 0) s.resources[idx].harvestedOrRemoved = true;
    }

    // --- Scene load hook: 복원 트리거 -----------------------------------
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬이 로드되면 해당 씬 안의 복원 담당자들이 자기 책임을 수행하게 합니다.
        // 예: SoilTilemapControllerRestorer, ResourceRestorer 같은 컴포넌트가
        // Start/Awake에서 WorldStateManager를 참조해 복원하도록 구성.
    }
}
