using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System.Threading.Tasks;
using UnityEngine;
using System;

public class CloudSaveService : MonoBehaviour
{
    FirebaseAuth auth;
    DatabaseReference root;
    FirebaseDatabase firebaseDatabase;

    [SerializeField]
    string databaseUrl =
    "https://stardewlike-default-rtdb.firebaseio.com/";

    public bool IsReady { get; private set; }
    public Task InitTask { get; private set; }

    void Awake()
    {
        InitTask = InitializeAsync();
    }

    async Task InitializeAsync()
    {
        try
        {
            var status = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (status != DependencyStatus.Available)
            {
                Debug.LogError("[CloudSave] Firebase deps: " + status);
                return;
            }

            auth = FirebaseAuth.DefaultInstance;
            if (auth.CurrentUser == null)
                await auth.SignInAnonymouslyAsync();

            var app = FirebaseApp.DefaultInstance;

            // ★ DefaultInstance 쓰지 말고 URL로 인스턴스 받기
            firebaseDatabase = FirebaseDatabase.GetInstance(app, databaseUrl);

            // (선택) 오프라인 캐시: 반드시 '위 한 줄'보다 먼저, 그리고 한 번만
            // db.SetPersistenceEnabled(true);

            root = firebaseDatabase.RootReference;
            IsReady = true;
            Debug.Log("[CloudSave] Initialized");
        }
        catch (Exception ex)
        {
            Debug.LogError("[CloudSave] Init failed: " + ex);
        }
    }

    string SlotPath(string slot) => $"users/{auth.CurrentUser.UserId}/saves/{slot}";

    public async Task SaveAsync(string slot, SaveData data)
    {
        if (!IsReady) await InitTask;
        if (!IsReady || root == null) throw new NullReferenceException("Firebase DB not ready.");

        string json = JsonUtility.ToJson(data);
        await root.Child(SlotPath(slot)).SetRawJsonValueAsync(json);
        Debug.Log($"[CloudSave] Saved -> {slot}");
    }

    public async Task<SaveData> LoadAsync(string slot)
    {
        if (!IsReady) await InitTask;
        if (!IsReady || root == null) throw new NullReferenceException("Firebase DB not ready.");

        var snap = await root.Child(SlotPath(slot)).GetValueAsync();
        if (!snap.Exists) return null;
        return JsonUtility.FromJson<SaveData>(snap.GetRawJsonValue());
    }
}
