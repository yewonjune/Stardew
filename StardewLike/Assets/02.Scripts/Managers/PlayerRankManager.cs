using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRankManager : MonoBehaviour
{
    public static PlayerRankManager Instance;

    [Header("Firebase")]
    public string databaseUrl = "https://stardewlike-default-rtdb.firebaseio.com/";
    private FirebaseAuth auth;
    private DatabaseReference root;
    bool initialized = false;

    [Header("UI - Panels")]
    public GameObject coopPanel;

    [Header("UI - Gold Rank (위)")]
    public Text goldRankTitleText;          // "현재 GOLD 순위"
    public Transform goldRankContent;       // 위 Content

    [Header("UI - NPC/Affection Rank (아래)")]
    public Text npcRankTitleText;           // "현재 호감도 순위"
    public Transform npcRankContent;        // 아래 Content

    [Header("UI - Left Info (선택)")]
    public Text emailText;
    public Text nicknameText;
    public Text farmNameText;
    public Text goldText;
    public Text seasonText;

    [Header("Prefab")]
    public GameObject rankBlockPrefab;      // RankBlock (안에 RankText, NameText, GoldText 있는 프리팹)

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        InitFirebase();
    }

    async void InitFirebase()
    {
        try
        {
            var status = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (status != DependencyStatus.Available)
            {
                Debug.LogError("[Rank] Firebase deps not available : " + status);
                return;
            }

            auth = FirebaseAuth.DefaultInstance;

            if (auth.CurrentUser == null)
            {
                Debug.LogWarning("[Rank] 로그인된 유저가 없어서 RankManager 초기화 스킵");
                return;
            }

            var app = FirebaseApp.DefaultInstance;
            var db = FirebaseDatabase.GetInstance(app, databaseUrl);
            root = db.RootReference;

            await EnsureUserDoc();

            initialized = true;

            await LoadMyUserInfo();
            Debug.Log("[Rank] Firebase init OK");
        }
        catch (Exception ex)
        {
            Debug.LogError("[Rank] Firebase init failed : " + ex);
        }
    }

    public async void OpenRankPanel()
    {
        if (!initialized)
        {
            // 초기화 기다리기
            Debug.Log("[Rank] 아직 초기화 중이라 기다립니다.");
            return; // 또는 여기서 코루틴으로 조금 있다 다시 호출
        }

        if (coopPanel) coopPanel.SetActive(true);

        await LoadMyUserInfo();
        // 내 데이터 올리고
        await UploadMyRankData();
        // 둘 다 갱신
        await RefreshBothRanks();
    }

    public void CloseRankPanel()
    {
        if (coopPanel) coopPanel.SetActive(false);
    }

    private async Task EnsureUserDoc()
    {
        if (auth == null || root == null) return;

        string uid = auth.CurrentUser.UserId;
        var userRef = root.Child("users").Child(uid);
        var snap = await userRef.GetValueAsync();

        string email = auth.CurrentUser.Email;
        if (string.IsNullOrEmpty(email)) email = "Guest";

        string displayName = auth.CurrentUser.DisplayName;
        if (string.IsNullOrEmpty(displayName))
            displayName = "Guest";

        // 기본값
        var updates = new Dictionary<string, object>();
        if (!snap.Exists)
        {
            updates["email"] = email;
            updates["farmName"] = "My Farm";
            updates["nickname"] = displayName;
            updates["season"] = "Spring";
            updates["gold"] = (PlayerWallet.Instance != null ? PlayerWallet.Instance.gold : 0);
            updates["createdAt"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            await userRef.UpdateChildrenAsync(updates);
            Debug.Log("[Rank] 새 유저 문서 생성 완료");
        }
        else
        {
            if (!snap.HasChild("farmName")) updates["farmName"] = "My Farm";
            if (!snap.HasChild("season")) updates["season"] = "Spring";
            if (!snap.HasChild("gold")) updates["gold"] = (PlayerWallet.Instance != null ? PlayerWallet.Instance.gold : 0);
            if (!snap.HasChild("nickname")) updates["nickname"] = displayName;
            if (updates.Count > 0)
            {
                await userRef.UpdateChildrenAsync(updates);
                Debug.Log("[Rank] 기존 유저 필드 보충 완료");
            }
        }
    }

    // DB에 저장할 기본 구조
    [Serializable]
    public class UserDTO
    {
        public string email;
        public string nickname;
        public string farmName;
        public string season;
        public int gold;
        public long createdAt;
    }
    private async Task LoadMyUserInfo()
    {
        if (auth == null || root == null) return;

        string uid = auth.CurrentUser.UserId;
        var snap = await root.Child("users").Child(uid).GetValueAsync();
        if (!snap.Exists)
        {
            Debug.Log("[Rank] no user info in DB");
            return;
        }

        string email = snap.Child("email").Value?.ToString() ?? "Guest";
        string nickname = snap.Child("nickname").Value?.ToString() ?? "";
        string farmName = snap.Child("farmName").Value?.ToString() ?? "Farm";
        string season = snap.Child("season").Value?.ToString() ?? "";
        string goldStr = snap.Child("gold").Value?.ToString() ?? "0";

        if (emailText) emailText.text = email;
        if (nicknameText) nicknameText.text = nickname;
        if (farmNameText) farmNameText.text = farmName;
        if (seasonText) seasonText.text = season;
        if (goldText) goldText.text = goldStr + " 골드";

        if (int.TryParse(goldStr, out int goldVal))
        {
            if (PlayerWallet.Instance != null)
            {
                PlayerWallet.Instance.gold = goldVal;
                PlayerWallet.Instance.RefreshUI();
            }
        }
    }
    public async Task SaveNickname(string nickname)
    {
        if (!initialized || auth == null || root == null) return;
        if (string.IsNullOrEmpty(nickname)) return;

        string uid = auth.CurrentUser.UserId;
        await root.Child("users").Child(uid).Child("nickname").SetValueAsync(nickname);
        Debug.Log("[Rank] 닉네임 저장 완료 : " + nickname);
    }

    public async Task UploadMyRankData()
    {
        if (!initialized || auth == null || root == null)
            return;

        string uid = auth.CurrentUser.UserId;
        string email = auth.CurrentUser.Email;
        if (string.IsNullOrEmpty(email)) email = "Guest";
        string nickname = (nicknameText != null) ? nicknameText.text : "";

        int gold = 0;
        if (PlayerWallet.Instance != null)
            gold = PlayerWallet.Instance.gold;

        int affectionTotal = CalcTotalNPCAffection();

        string farmName = (farmNameText != null) ? farmNameText.text : "Farm";
        string season = (seasonText != null) ? seasonText.text : "";

        PlayerRankDTO dto = new PlayerRankDTO
        {
            uid = uid,
            email = email,
            nickname = nickname,
            gold = gold,
            affection = affectionTotal,
            farmName = farmName,
            season = season,
            updatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        string json = JsonUtility.ToJson(dto);
        await root.Child("ranks").Child(uid).SetRawJsonValueAsync(json);

        // 왼쪽 정보 UI도 채우기
        if (emailText != null) emailText.text = email;
        if (goldText != null) goldText.text = gold + " 골드";
    }

    public async Task RefreshBothRanks()
    {
        if (!initialized || root == null)
            return;

        // 제목 세팅
        if (goldRankTitleText != null)
            goldRankTitleText.text = "현재 GOLD 순위";
        if (npcRankTitleText != null)
            npcRankTitleText.text = "현재 호감도 순위";

        var snap = await root.Child("ranks").GetValueAsync();
        if (!snap.Exists)
        {
            ClearContent(goldRankContent);
            ClearContent(npcRankContent);
            return;
        }

        // 전체 데이터 한 번에 파싱
        List<PlayerRankDTO> all = new List<PlayerRankDTO>();
        foreach (var child in snap.Children)
        {
            try
            {
                var json = child.GetRawJsonValue();
                var dto = JsonUtility.FromJson<PlayerRankDTO>(json);
                if (dto != null)
                    all.Add(dto);
            }
            catch { }
        }

        // 위쪽: 골드 기준 내림차순
        var goldList = all.OrderByDescending(x => x.gold).ToList();
        // 아래쪽: 호감도 기준 내림차순
        var npcList = all.OrderByDescending(x => x.affection).ToList();

        // UI 비우고
        ClearContent(goldRankContent);
        ClearContent(npcRankContent);

        // 위쪽 뿌리기
        int rank = 1;
        foreach (var dto in goldList)
        {
            CreateRankBlockLine(goldRankContent, rank, dto, isGold: true);
            rank++;
        }

        // 아래쪽 뿌리기
        int npcRank = 1;
        foreach (var dto in npcList)
        {
            CreateRankBlockLine(npcRankContent, npcRank, dto, isGold: false);
            npcRank++;
        }
    }

    void ClearContent(Transform content)
    {
        if (content == null) return;
        for (int i = content.childCount - 1; i >= 0; i--)
            GameObject.Destroy(content.GetChild(i).gameObject);
    }

    void CreateRankBlockLine(Transform parent, int rank, PlayerRankDTO dto, bool isGold)
    {
        if (rankBlockPrefab == null || parent == null) return;

        GameObject go = Instantiate(rankBlockPrefab, parent);

        // 프리팹 안 Text들 이름으로 찾아오기
        Text[] texts = go.GetComponentsInChildren<Text>(true);

        Text rankText = null;
        Text nameText = null;
        Text valueText = null;

        foreach (var tx in texts)
        {
            if (tx.name == "RankText") rankText = tx;
            else if (tx.name == "NameText") nameText = tx;
            else if (tx.name == "GoldText") valueText = tx;
        }

        if (rankText != null)
            rankText.text = rank.ToString() + ")";

        if (nameText != null)
        {
            if (!string.IsNullOrEmpty(dto.nickname))
                nameText.text = dto.nickname;
            else if (!string.IsNullOrEmpty(dto.email))
                nameText.text = dto.email;
            else
                nameText.text = "Guest";
        }

        if (valueText != null)
        {
            if (isGold)
                valueText.text = "Gold: " + dto.gold;
            else
                valueText.text = "Affection: " + dto.affection;
        }
    }

    int CalcTotalNPCAffection()
    {
        int sum = 0;
        var all = FindObjectsOfType<NPCAffection>();
        foreach (var a in all)
            sum += a.currentAffection;
        return sum;
    }

    [Serializable]
    public class PlayerRankDTO
    {
        public string uid;
        public string email;
        public string nickname;
        public string farmName;
        public string season;
        public int gold;
        public int affection;
        public long updatedAt;
    }
}
