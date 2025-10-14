using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AreaPortal : MonoBehaviour
{
    // 인스펙터에서 설정
    public string targetScene;
    public string spawnPointId;

    public bool freezePlayerDuringTransition = true;
    public float fadeDuration = 0.2f;
    public float gateCooldown = 0.6f;

    static float s_CooldownUntil = 0f;
    bool isTransitioning;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;
        if (Time.time < s_CooldownUntil) return;
        if (isTransitioning) return;

        StartCoroutine(SwitchAreaRoutine());
    }

    IEnumerator SwitchAreaRoutine()
    {
        isTransitioning = true;

        s_CooldownUntil = Time.time + gateCooldown;

        var prevActive = SceneManager.GetActiveScene();
        var prevActiveName = prevActive.name;

        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (!playerGO)
        {
            isTransitioning = false;
            yield break;
        }
        var player = playerGO.transform;
        var rb = player.GetComponent<Rigidbody2D>();
        var col = player.GetComponent<Collider2D>();
        var anim = player.GetComponent<Animator>();
        var mover = player.GetComponent<PlayerMovement>();

        if (freezePlayerDuringTransition)
        {
            if (mover) mover.SetControl(false);
            if (rb) { rb.velocity = Vector2.zero; rb.simulated = false; }
            if (col) col.enabled = false;
            if (anim) anim.SetBool("isMoving", false);
        }

        yield return FadeManager.Instance.FadeOut(fadeDuration);

        var load = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);
        yield return load;

        var target = SceneManager.GetSceneByName(targetScene);
        if (!target.IsValid())
        {
            Debug.LogError($"[AreaPortal] Target scene '{targetScene}' not found.");
        }
        else
        {
            SceneManager.SetActiveScene(target);
        }

        PlayerSpawnManager.NextSpawnPointId = spawnPointId;
        PlayerSpawnManager.Instance.PlacePlayerAtSpawn();
        
        var gateCol = GetComponent<Collider2D>();
        if (gateCol) gateCol.enabled = false;

        yield return null;
        if (gateCol) gateCol.enabled = true;

        var vcams = FindObjectsOfType<CinemachineVirtualCamera>(includeInactive: true);
        foreach (var v in vcams)
        {
            if (v && v.Follow == null) v.Follow = player;
        }
        yield return null;
        foreach (var v in vcams) v.GetComponent<CinemachineConfiner2D>()?.InvalidateCache();

        yield return new WaitForSecondsRealtime(0.1f);

        if (rb) { rb.simulated = true; rb.WakeUp(); }
        if (col) col.enabled = true;
        if (mover) mover.SetControl(true);

        yield return null;
        if (gateCol) gateCol.enabled = true;

        FadeManager.Instance.FadeInCoroutine(fadeDuration);

        if (!string.IsNullOrEmpty(prevActiveName))
        {
            var unload = SceneManager.UnloadSceneAsync(prevActiveName);
            if (unload != null) yield return unload;
        }
        yield return Resources.UnloadUnusedAssets();



        isTransitioning = false;
    }
}
