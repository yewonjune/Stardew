using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlaceSceneGate : MonoBehaviour
{
    static float s_GateCooldownUntil = 0f;

    public Transform player;
    public CameraManager cameraManager;

    public string targetSceneName;
    public string targetSpawnPointName = "EntrancePoint";

    public bool unloadActiveMapScene = true;

    public bool freezePlayerDuringTransition = true;

    bool isTransitioning;

    void Awake()
    {
        if (!player)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go) player = go.transform;
        }
        if (!cameraManager) cameraManager = CameraManager.Instance;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.time < s_GateCooldownUntil) return;

        if (other.CompareTag("Player"))
            StartSceneTransition();
    }

    public void StartSceneTransition()
    {
        if (isTransitioning) return;

        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("[PlaceSceneGate] targetSceneName이 비어있습니다.");
            return;
        }

        isTransitioning = true;
        StartCoroutine(DoSceneSwap());
    }

    IEnumerator DoSceneSwap()
    {
        if (!player)
        {
            Debug.LogError("[PlaceSceneGate] player가 없습니다.");
            isTransitioning = false;
            yield break;
        }

        var rb = player.GetComponent<Rigidbody2D>();
        var col = player.GetComponent<Collider2D>();
        var mover = player.GetComponent<PlayerMovement>();
        var anim = player.GetComponent<Animator>();

        if (freezePlayerDuringTransition)
        {
            if (mover) mover.SetControl(false);
            if (rb) { rb.velocity = Vector2.zero; rb.simulated = false; }
            if (col) col.enabled = false;
            if (anim) { anim.SetBool("isMoving", false); }
        }

        FadeManager.Instance.FadeOutIn(() => { /* no-op */ });

        Scene activeScene = SceneManager.GetActiveScene();
        string activeSceneName = activeScene.name;

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
        yield return loadOp;

        Scene target = SceneManager.GetSceneByName(targetSceneName);
        if (target.IsValid())
            SceneManager.SetActiveScene(target);
        else
        {
            Debug.LogError($"[PlaceSceneGate] 타겟 씬({targetSceneName})을 찾을 수 없습니다.");
        }

        Transform spawn = FindSpawnPointInScene(targetSpawnPointName);

        if (spawn != null && player != null)
            player.position = spawn.position;
        else
            Debug.LogWarning($"[PlaceSceneGate] 스폰 포인트 '{targetSpawnPointName}' 를 찾지 못했습니다. 플레이어 위치 이동 생략.");

        if (cameraManager != null)
        {
            // 예시) 씬 이름에 따라 스위치 메서드 선택
            if (targetSceneName.Contains("Village"))
                cameraManager.SwitchToVillage();
            else if (targetSceneName.Contains("Farm"))
                cameraManager.SwitchToFarm();
            else
                cameraManager.SwitchToFarm();
        }

        var vcams = FindObjectsOfType<CinemachineVirtualCamera>(true);
        foreach (var v in vcams)
            v.GetComponent<CinemachineConfiner2D>()?.InvalidateCache();

        // 7) 한 프레임 대기 후 Confiner 캐시(2차)
        yield return null;
        foreach (var v in vcams)
            v.GetComponent<CinemachineConfiner2D>()?.InvalidateCache();

        // 8) 플레이어 복구(언로드 전에!)
        s_GateCooldownUntil = Time.time + 0.6f; // 재트리거 방지
        Time.timeScale = 1f;                    // 혹시 모를 정지 해제

        if (rb) { rb.simulated = true; rb.WakeUp(); rb.bodyType = RigidbodyType2D.Dynamic; }
        if (col) col.enabled = true;
        if (mover) mover.SetControl(true);

        // 9) 이전 맵 씬 언로드(기다리지 않고 비동기 실행) → 이 컴포넌트가 파괴되어도 이미 복구 끝난 상태
        if (unloadActiveMapScene && activeSceneName != "ManagerScene")
            SceneManager.UnloadSceneAsync(activeSceneName);


        isTransitioning = false;
    }

    Transform FindSpawnPointInScene(string spawnName)
    {
        // 활성 씬에서만 찾습니다(위에서 SetActiveScene으로 전환함)
        var gos = GameObject.FindObjectsOfType<Transform>(true);
        foreach (var t in gos)
        {
            if (t.name == spawnName)
                return t;
        }
        return null;
    }
}
