using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class NPCStateManager : MonoBehaviour
{
    public static NPCStateManager Instance { get; private set; }

    [System.Serializable]
    public struct NPCRuntimeState
    {
        public int affection;           // 호감도
        public int waypointIndex;       // 현재 웨이포인트 인덱스
        public bool hasWaypointProgress;// 진행도 저장 여부
        public Vector3 position;        // 마지막 알려진 위치
        public string scene;            // 마지막 알려진 씬 이름
        public bool hasPosition;        // 위치 저장 여부
        // 저장 여부 판단용(옵션): JSON 부재 시 false
        public bool hasAffectionSaved;  // 호감도 저장 여부(디스크에서 불러왔는지)
    }

    private readonly Dictionary<string, NPCRuntimeState> _states = new Dictionary<string, NPCRuntimeState>();

    // PlayerPrefs 키 생성
    private static string Key(string npcId) => $"NPCState_{npcId}";

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // --- 내부: 디스크 저장/로드 ---
    private bool TryLoadFromPrefs(string npcId, out NPCRuntimeState state)
    {
        var key = Key(npcId);
        if (!PlayerPrefs.HasKey(key))
        {
            state = default;
            return false;
        }

        var json = PlayerPrefs.GetString(key, string.Empty);
        if (string.IsNullOrEmpty(json))
        {
            state = default;
            return false;
        }

        state = JsonUtility.FromJson<NPCRuntimeState>(json);
        return true;
    }

    private void SaveToPrefs(string npcId, NPCRuntimeState state)
    {
        var json = JsonUtility.ToJson(state);
        PlayerPrefs.SetString(Key(npcId), json);
        PlayerPrefs.Save();
    }

    private NPCRuntimeState GetState(string npcId)
    {
        // 1) 메모리에 있으면 그대로
        if (_states.TryGetValue(npcId, out var s)) return s;

        // 2) 디스크(PlayerPrefs)에서 시도
        if (TryLoadFromPrefs(npcId, out var loaded))
        {
            // 디스크에서 불러온 건 저장 플래그를 확실히
            loaded.hasAffectionSaved |= true;
            _states[npcId] = loaded;
            return loaded;
        }

        // 3) 아무것도 없으면 디폴트 생성
        var created = new NPCRuntimeState
        {
            affection = 0,
            waypointIndex = 0,
            hasWaypointProgress = false,
            position = Vector3.zero,
            scene = null,
            hasPosition = false,
            hasAffectionSaved = false
        };
        _states[npcId] = created;
        return created;
    }

    private void SetState(string npcId, NPCRuntimeState s, bool persist = true)
    {
        _states[npcId] = s;
        if (persist)
            SaveToPrefs(npcId, s);
    }

    // ----- Affection -----
    public int LoadAffection(string npcId, int defaultValue = 0)
    {
        var s = GetState(npcId);

        // 디스크/메모리에 저장된 적 없다면 default 반환
        if (!s.hasAffectionSaved && s.affection == 0 && !PlayerPrefs.HasKey(Key(npcId)))
            return defaultValue;

        return s.affection;
    }

    public void SaveAffection(string npcId, int value)
    {
        var s = GetState(npcId);
        s.affection = value;
        s.hasAffectionSaved = true;
        SetState(npcId, s, persist: true);
    }

    // ----- Waypoint Progress -----
    public int LoadWaypointIndex(string npcId, int defaultValue = 0)
    {
        var s = GetState(npcId);
        return s.hasWaypointProgress ? s.waypointIndex : defaultValue;
    }

    public void SaveWaypointIndex(string npcId, int index)
    {
        var s = GetState(npcId);
        s.waypointIndex = index;
        s.hasWaypointProgress = true;
        SetState(npcId, s, persist: true);
    }

    public bool HasWaypointProgress(string npcId)
    {
        var s = GetState(npcId);
        return s.hasWaypointProgress;
    }

    // ----- Position -----
    public bool TryLoadPosition(string npcId, out Vector3 position)
    {
        var s = GetState(npcId);
        if (s.hasPosition)
        {
            position = s.position;
            return true;
        }
        position = default;
        return false;
    }

    public void SavePosition(string npcId, Vector3 position)
    {
        var s = GetState(npcId);
        s.position = position;
        s.hasPosition = true;
        SetState(npcId, s, persist: true);
    }

    // ----- Scene -----
    public string LoadScene(string npcId, string defaultValue = null)
    {
        var s = GetState(npcId);
        return s.scene ?? defaultValue;
    }

    public void SaveScene(string npcId, string sceneName)
    {
        var s = GetState(npcId);
        s.scene = sceneName;
        SetState(npcId, s, persist: true);
    }

    // 하루 초기화 유틸(선택)
    public void ResetAllForNewDay(bool keepAffection = true)
    {
        var keys = new List<string>(_states.Keys);
        foreach (var id in keys)
        {
            var s = _states[id];
            if (!keepAffection)
            {
                s.affection = 0;
                s.hasAffectionSaved = true; // 0으로 초기화된 값도 저장
            }
            s.waypointIndex = 0;
            s.hasWaypointProgress = false;
            s.hasPosition = false;

            // 메모리와 디스크 모두에 반영
            _states[id] = s;
            SaveToPrefs(id, s);
        }
    }

    void OnApplicationQuit()
    {
        // 혹시 모를 누락 방지: 메모리에 있는 것들 전부 동기화
        foreach (var kv in _states)
            SaveToPrefs(kv.Key, kv.Value);
    }
}
