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
        if (!initialized)
        {
            return;
        }

        if (coopPanel) coopPanel.SetActive(true);

        await LoadMyUserInfo();
        await UploadMyRankData();
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

        var updates = new Dictionary<string, object>();
        if (!snap.Exists)
        {
            updates["email"] = email;
            updates["farmName"] = "My Farm";
            updates["nickname"] = displayName;
            updates["season"] = "Spring";
            updates["createdAt"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            await userRef.UpdateChildrenAsync(updates);
            Debug.Log("[Rank] 새 유저 문서 생성 완료");
        }
        else
        {
            if (!snap.HasChild("farmName")) updates["farmName"] = "My Farm";
            if (!snap.HasChild("season")) updates["season"] = "Spring";
            if (!snap.HasChild("nickname")) updates["nickname"] = displayName;
            if (updates.Count > 0)
            {
                await userRef.UpdateChildrenAsync(updates);
            }
        }
    }

    [Serializable]
    public class UserDTO
    {
        public string email;
        public string nickname;
        public string farmName;
        public string season;
        public long createdAt;
    }
    private async Task LoadMyUserInfo()
    {
        if (auth == null || root == null) return;

        string uid = auth.CurrentUser.UserId;
        var snap = await root.Child("users").Child(uid).GetValueAsync();
        if (!snap.Exists)
        {
            return;
        }

        string email = snap.Child("email").Value?.ToString() ?? "Guest";
        string nickname = snap.Child("nickname").Value?.ToString() ?? "";
        string farmName = snap.Child("farmName").Value?.ToString() ?? "Farm";
        string season = snap.Child("season").Value?.ToString() ?? "";
        
        string goldStr = "0";
        var saveMetaGold = snap.Child("saves").Child("slot1").Child("meta").Child("gold");
        if (saveMetaGold.Exists && saveMetaGold.Value != null)
        {
            goldStr = saveMetaGold.Value.ToString();
        }
        else
        {
            goldStr = snap.Child("gold").Value?.ToString() ?? "0";
        }

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
        var goldSnap = await root
            .Child("users")
            .Child(uid)
            .Child("saves")
            .Child("slot1")
            .Child("meta")
            .Child("gold")
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


        string farmName = (farmNameText != null) ? farmNameText.text : "Farm";
        string season = (seasonText != null) ? seasonText.text : "";

        PlayerRankDTO dto = new PlayerRankDTO
        {
            uid = uid,
            email = email,
            nickname = nickname,
            gold = gold,
            farmName = farmName,
            season = season,
            updatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        string json = JsonUtility.ToJson(dto);
        await root.Child("ranks").Child(uid).SetRawJsonValueAsync(json);

        if (emailText != null) emailText.text = email;
        if (goldText != null) goldText.text = gold + " 골드";
    }

    public async Task RefreshBothRanks()
    {
        if (!initialized || root == null)
            return;

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
                catch { }
            }
        }

        var usersSnap = await root.Child("users").GetValueAsync();
        if (usersSnap.Exists)
        {
            foreach (var user in usersSnap.Children)
            {
                string uid = user.Key;
                var target = all.FirstOrDefault(x => x.uid == uid);
                if (target == null)
                    continue;

                var nk = user.Child("nickname");
                if (nk.Exists && nk.Value != null)
                    target.nickname = nk.Value.ToString();

                int latestGold = target.gold;
                var saveGold = user.Child("saves").Child("slot1").Child("meta").Child("gold");
                if (saveGold.Exists && saveGold.Value != null)
                {
                    int.TryParse(saveGold.Value.ToString(), out latestGold);
                }
                else
                {
                    var rootGold = user.Child("gold");
                    if (rootGold.Exists && rootGold.Value != null)
                        int.TryParse(rootGold.Value.ToString(), out latestGold);
                }
                target.gold = latestGold;
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
            GameObject.Destroy(content.GetChild(i).gameObject);
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
        }
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
        public long updatedAt;
    }
}
