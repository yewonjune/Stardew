using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeathFlow : MonoBehaviour
{
    [SerializeField] PlayerHealthController hp;
    [SerializeField] Animator animator;
    Rigidbody2D rb;

    [SerializeField] float dieAnimWait = 0.8f;
    [SerializeField] float fadeDuration = 1.0f;

    [SerializeField] Transform bedSpawnPoint;

    const string FARM_SCENE = "FarmScene";

    bool running;

    void Awake()
    {
        if (!hp) hp = GetComponent<PlayerHealthController>();
        if (!animator) animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        if (hp != null) hp.OnDead += StartDeadFlow;
    }

    void OnDisable()
    {
        if (hp != null) hp.OnDead -= StartDeadFlow;
    }

    void StartDeadFlow()
    {
        if (running) return;

        running = true;
        StartCoroutine(CoDeadFlow());
    }

    IEnumerator CoDeadFlow()
    {
        PlayerActionLock.Lock("Dead");

        if (animator) animator.SetTrigger("Die");

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        yield return new WaitForSeconds(dieAnimWait);

        if (FadeManager.Instance != null)
            yield return FadeManager.Instance.FadeOut(fadeDuration);

        if (!IsSceneLoaded(FARM_SCENE))
        {
            yield return SceneManager.LoadSceneAsync(FARM_SCENE, LoadSceneMode.Additive);
        }

        yield return SetActiveSceneByName(FARM_SCENE);

        bedSpawnPoint = GameObject.Find("BedSpawnPoint")?.transform;
        if (bedSpawnPoint == null)
        {
            Debug.LogError("[DeathFlow] BedSpawnPoint not found in FarmScene!");
            PlayerActionLock.Unlock("Dead");
            running = false;
            yield break;
        }

        var tm = FindObjectOfType<TimeManager>(true);
        if (tm != null)
        {
            tm.EndDay();
        }

        TeleportTo(bedSpawnPoint.position);

        CameraManager.Instance?.SwitchTo("House");

        CaveStateManager.ResetToEntrance();

        yield return UnloadSceneIfLoaded("CaveScene");

        if (hp != null)
        {
            // 완전 회복
            hp.SetHp(hp.maxHP);
        }

        if (animator)
        {
            animator.ResetTrigger("Die");
            animator.Play("PlayerIdle");
        }

        if (FadeManager.Instance != null)
            yield return FadeManager.Instance.FadeIn(fadeDuration);

        PlayerActionLock.Unlock("Dead");
        running = false;
    }

    void TeleportTo(Vector2 pos)
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.position = pos;
        }
        else
        {
            transform.position = pos;
        }
    }
    bool IsSceneLoaded(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.isLoaded && s.name == sceneName)
                return true;
        }
        return false;
    }

    IEnumerator SetActiveSceneByName(string sceneName)
    {
        Scene target = default;

        while (true)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                if (s.isLoaded && s.name == sceneName)
                {
                    target = s;
                    break;
                }
            }

            if (target.IsValid() && target.isLoaded)
                break;

            yield return null;
        }

        SceneManager.SetActiveScene(target);
    }

    IEnumerator UnloadSceneIfLoaded(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) yield break;

        // 로드 여부 확인
        Scene target = default;
        bool found = false;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.isLoaded && s.name == sceneName)
            {
                target = s;
                found = true;
                break;
            }
        }
        if (!found) yield break;

        // 언로드
        var op = SceneManager.UnloadSceneAsync(target);
        if (op == null) yield break;

        while (!op.isDone) yield return null;
    }
}
