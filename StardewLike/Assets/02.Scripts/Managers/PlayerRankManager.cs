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
    public Text goldRankTitleText;
    public Transform goldRankContent;

    [Header("UI - Left Info (선택)")]
    public Text emailText;
    public Text nicknameText;
    public Text farmNameText;
    public Text goldText;

    // NOTE: 기존 seasonText 슬롯을 그대로 사용해 "N일차" 표기
    public Text seasonText;

    [Header("Prefab")]
    public GameObject goldRankBlockPrefab;

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
                return;
            }

            auth = FirebaseAuth.DefaultInstance;
            if (auth.CurrentUser == null)
            {
                return;
            }

            var app = FirebaseApp.DefaultInstance;
            var db = FirebaseDatabase.GetInstance(app, databaseUrl);
            root = db.RootReference;

            await EnsureUserDoc();

            initialized = true;

            await LoadMyUserInfo();
        }
        catch (Exception ex)
        {
            Debug.LogError("[Rank] Firebase init failed : " + ex);
        }
    }

    public async void OpenRankPanel()
    {
        // 1. 먼저 패널부터 켜기
        if (coopPanel != null)
            coopPanel.SetActive(true);
        else
            Debug.LogWarning("[Rank] coopPanel 이 바인딩되어 있지 않음");

        // 2. Firebase가 아직 준비 안 됐으면 여기서 끝 (패널은 열린 상태 유지)
        if (!initialized)
        {
            Debug.LogWarning("[Rank] 아직 Firebase가 초기화되지 않아서 랭킹 데이터를 불러오지 못함");
            return;
        }

        // 3. 준비되어 있으면 데이터 로드
        await LoadMyUserInfo();
        await UploadMyRankData();
        await RefreshGoldRank();
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
        if (string.IsNullOrEmpty(displayName)) displayName = "Guest";

        var updates = new Dictionary<string, object>();

        if (!snap.Exists)
        {
            updates["email"] = email;
            updates["farmName"] = "My Farm";
            updates["nickname"] = displayName;
            updates["createdAt"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            await userRef.UpdateChildrenAsync(updates);
            Debug.Log("[Rank] 새 유저 문서 생성 완료");
        }
        else
        {
            if (!snap.HasChild("farmName")) updates["farmName"] = "My Farm";
            if (!snap.HasChild("nickname")) updates["nickname"] = displayName;

            if (updates.Count > 0)
            {
                await userRef.UpdateChildrenAsync(updates);
            }
        }
    }

    private async Task LoadMyUserInfo()
    {
        if (auth == null || root == null) return;

        string uid = auth.CurrentUser.UserId;
        var snap = await root.Child("users").Child(uid).GetValueAsync();
        if (!snap.Exists) return;

        string email = snap.Child("email").Value?.ToString() ?? "Guest";
        string nickname = snap.Child("nickname").Value?.ToString() ?? "";
        string farmName = snap.Child("farmName").Value?.ToString() ?? "Farm";

        int day = 1;
        var metaDayNode = snap.Child("saves").Child("slot1").Child("meta").Child("day");
        if (metaDayNode.Exists && metaDayNode.Value != null)
        {
            int.TryParse(metaDayNode.Value.ToString(), out day);
        }

        int goldVal = 0;
        var saveMetaGold = snap.Child("saves").Child("slot1").Child("meta").Child("gold");
        if (saveMetaGold.Exists && saveMetaGold.Value != null)
            int.TryParse(saveMetaGold.Value.ToString(), out goldVal);
        else if (PlayerWallet.Instance != null)
            goldVal = PlayerWallet.Instance.gold;

        if (emailText) emailText.text = email;
        if (nicknameText) nicknameText.text = nickname;
        if (farmNameText) farmNameText.text = farmName;
        if (seasonText) seasonText.text = GetSeasonDayText(day);
        if (goldText) goldText.text = goldVal + " 골드";
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
        if (!initialized || auth == null || root == null) return;

        string uid = auth.CurrentUser.UserId;
        string email = auth.CurrentUser.Email;
        if (string.IsNullOrEmpty(email)) email = "Guest";
        string nickname = (nicknameText != null) ? nicknameText.text : "";

        int gold = 0;
        var goldSnap = await root
            .Child("users").Child(uid)
            .Child("saves").Child("slot1").Child("meta").Child("gold")
            .GetValueAsync();

        if (goldSnap.Exists && goldSnap.Value != null)
        {
            int.TryParse(goldSnap.Value.ToString(), out gold);
        }
        else
        {
            if (PlayerWallet.Instance != null)
                gold = PlayerWallet.Instance.gold;
        }

        int day = 1;
        var metaDaySnap = await root
            .Child("users").Child(uid)
            .Child("saves").Child("slot1").Child("meta").Child("day")
            .GetValueAsync();

        if (metaDaySnap.Exists && metaDaySnap.Value != null)
        {
            int.TryParse(metaDaySnap.Value.ToString(), out day);
        }

        string farmName = (farmNameText != null) ? farmNameText.text : "Farm";

        PlayerRankDTO dto = new PlayerRankDTO
        {
            uid = uid,
            email = email,
            nickname = nickname,
            gold = gold,
            farmName = farmName,
            day = day,
            updatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        string json = JsonUtility.ToJson(dto);
        await root.Child("ranks").Child(uid).SetRawJsonValueAsync(json);

        if (emailText != null) emailText.text = email;
        if (goldText != null) goldText.text = gold + " 골드";
        if (seasonText) seasonText.text = GetSeasonDayText(day);
    }

    public async Task RefreshGoldRank()
    {
        if (!initialized || root == null) return;

        if (goldRankTitleText != null)
            goldRankTitleText.text = "현재 GOLD 순위";

        var ranksSnap = await root.Child("ranks").GetValueAsync();
        List<PlayerRankDTO> all = new List<PlayerRankDTO>();

        if (ranksSnap.Exists)
        {
            foreach (var child in ranksSnap.Children)
            {
                try
                {
                    var json = child.GetRawJsonValue();
                    var dto = JsonUtility.FromJson<PlayerRankDTO>(json);
                    if (dto != null)
                        all.Add(dto);
                }
                catch
                {
                    // 무시
                }
            }
        }

        var goldList = all.OrderByDescending(x => x.gold).ToList();

        ClearContent(goldRankContent);

        int rank = 1;
        foreach (var dto in goldList)
        {
            CreateRankBlockLine(goldRankContent, rank, dto, isGold: true, prefab: goldRankBlockPrefab);
            rank++;
        }
    }

    void ClearContent(Transform content)
    {
        if (content == null) return;
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);
    }

    void CreateRankBlockLine(Transform parent, int rank, PlayerRankDTO dto, bool isGold, GameObject prefab)
    {
        if (parent == null) return;
        if (prefab == null)
        {
            Debug.LogWarning("[Rank] prefab is null for " + (isGold ? "gold" : "npc"));
            return;
        }

        GameObject go = Instantiate(prefab, parent);

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
            rankText.text = rank.ToString();

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
        }
    }

    [Serializable]
    public class PlayerRankDTO
    {
        public string uid;
        public string email;
        public string nickname;
        public string farmName;
        public int day;
        public int gold;
        public long updatedAt;
    }

    public void OpenCoopPanelFromBoard()
    {
        OpenRankPanel();
    }

    public void BindUI(
    GameObject coopPanel,
    Text emailText,
    Text nicknameText,
    Text farmNameText,
    Text goldText,
    Text seasonText,
    Transform goldRankContent,
    GameObject goldRankBlockPrefab
)
    {
        // FarmScene에서 보내준 UI 레퍼런스를 여기 저장
        this.coopPanel = coopPanel;
        this.emailText = emailText;
        this.nicknameText = nicknameText;
        this.farmNameText = farmNameText;
        this.goldText = goldText;
        this.seasonText = seasonText;
        this.goldRankContent = goldRankContent;
        this.goldRankBlockPrefab = goldRankBlockPrefab;
    }

    private string GetSeasonDayText(int day)
    {
        int dayOfYear = (day - 1) % 120;
        int seasonIndex = dayOfYear / 30;
        int dayInSeason = (dayOfYear % 30) + 1;

        string seasonKorean = "";
        switch (seasonIndex)
        {
            case 0: seasonKorean = "봄"; break;
            case 1: seasonKorean = "여름"; break;
            case 2: seasonKorean = "가을"; break;
            case 3: seasonKorean = "겨울"; break;
        }

        return $"{seasonKorean} {dayInSeason}일차";
    }
}
