using UnityEngine;

public class NPCAffection : MonoBehaviour
{
    [Header("Identity")]
    public string npcId;

    [Header("Affection")]
    public int maxAffection = 100;
    public int currentAffection = 0;

    [Header("Options")]
    public bool loadOnStart = true;
    public bool autoSaveOnChange = true;

    // PlayerPrefs 키 이름 자동 생성
    string PrefKey => $"Affection_{npcId}";

    void Awake()
    {
        // Start 전에 먼저 로드해서 초기 affection이 0으로 표시되지 않도록
        if (loadOnStart)
            Load();
    }

    void Start()
    {
        currentAffection = Mathf.Clamp(currentAffection, 0, maxAffection);
    }

    public void SetAffection(int value)
    {
        int clamped = Mathf.Clamp(value, 0, maxAffection);
        if (clamped == currentAffection) return;

        currentAffection = clamped;
        if (autoSaveOnChange)
            Save();
    }

    public void AddAffection(int amount)
    {
        SetAffection(currentAffection + amount);
    }

    public void Save()
    {
        if (string.IsNullOrEmpty(npcId))
            return;

        // NPCStateManager가 있으면 우선 저장
        if (NPCStateManager.Instance != null)
        {
            NPCStateManager.Instance.SaveAffection(npcId, currentAffection);
        }
        else
        {
            // 없으면 로컬에 폴백 저장
            PlayerPrefs.SetInt(PrefKey, currentAffection);
            PlayerPrefs.Save();
        }
    }

    public void Load()
    {
        if (string.IsNullOrEmpty(npcId))
            return;

        int loadedValue = currentAffection;

        // NPCStateManager가 있으면 우선 거기서 로드
        if (NPCStateManager.Instance != null)
        {
            loadedValue = NPCStateManager.Instance.LoadAffection(npcId, currentAffection);
        }
        else
        {
            // 없으면 PlayerPrefs에서 로드
            loadedValue = PlayerPrefs.GetInt(PrefKey, currentAffection);
        }

        currentAffection = Mathf.Clamp(loadedValue, 0, maxAffection);
    }

    void OnDisable()
    {
        Save(); // 씬 전환 시에도 안전하게 저장
    }

    void OnApplicationQuit()
    {
        Save(); // 게임 종료 시에도 저장
    }
}
