using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    [SerializeField] int activePriority = 20;
    [SerializeField] int idlePriority = 0;

    [SerializeField] string playerTag = "Player";
    [SerializeField] string playerCameraTargetName = "CameraTarget";

    readonly Dictionary<string, CinemachineVirtualCamera> vcams = new();

    Transform followTarget;
    string currentKey;

    void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        RebindFollowTarget();
        RescanAndRegisterAll();
        if (!string.IsNullOrEmpty(currentKey)) SwitchTo(currentKey);
    }

    void OnActiveSceneChanged(Scene prev, Scene next)
    {
        RebindFollowTarget();
        RescanAndRegisterAll();
        if (!string.IsNullOrEmpty(currentKey)) SwitchTo(currentKey);
    }

    public void RegisterVCam(string key, CinemachineVirtualCamera vcam)
    {
        string k = key?.Trim();
        if (string.IsNullOrEmpty(k) || !vcam) return;

        vcams[k] = vcam;
        ApplyFollowLookAt(vcam);
        SetPriority(vcam, idlePriority);

        if (!string.IsNullOrEmpty(currentKey) && k == currentKey)
            SetPriority(vcam, activePriority);
    }

    public void UnregisterVCam(string key, CinemachineVirtualCamera vcam)
    {
        string k = key?.Trim();
        if (string.IsNullOrEmpty(k)) return;

        if (vcams.TryGetValue(k, out var cur) && cur == vcam)
            vcams.Remove(k);
    }

    public bool SwitchTo(string key)
    {
        var k = key?.Trim();
        if (string.IsNullOrEmpty(k)) return false;

        if (!vcams.TryGetValue(k, out var target) || !target)
        {
            RescanAndRegisterAll();
        }

        if (!vcams.TryGetValue(k, out target) || !target)
        {
            Debug.LogWarning($"[CameraManager] No VCam registered for '{k}'.");
            DebugPrintRegisteredKeys();
            return false;
        }

        // 우선순위 정리
        foreach (var kv in vcams)
            if (kv.Value) kv.Value.Priority = idlePriority;

        target.Priority = activePriority;
        currentKey = k;
        return true;
    }

    void RebindFollowTarget()
    {
        var playerGO = GameObject.FindGameObjectWithTag(playerTag);
        if (!playerGO) return;

        var t = playerGO.transform.Find(playerCameraTargetName);
        followTarget = t ? t : playerGO.transform;

        foreach (var v in vcams.Values)
            ApplyFollowLookAt(v);
    }

    void ApplyFollowLookAt(CinemachineVirtualCamera vcam)
    {
        if (!vcam) return;
        if (followTarget)
        {
            if (vcam.Follow != followTarget) vcam.Follow = followTarget;
            if (vcam.LookAt != followTarget) vcam.LookAt = followTarget;
        }
        vcam.GetComponent<CinemachineConfiner2D>()?.InvalidateCache();
    }

    static void SetPriority(CinemachineVirtualCamera vcam, int p)
    {
        if (vcam && vcam.Priority != p) vcam.Priority = p;
    }
    public void RescanAndRegisterAll()
    {
        var regs = GameObject.FindObjectsOfType<VCamRegister>(true);
        foreach (var reg in regs)
        {
            if (!reg || !reg.isActiveAndEnabled) continue;

            var k = reg.key?.Trim();
            if (string.IsNullOrEmpty(k)) continue;

            var v = reg.GetComponent<CinemachineVirtualCamera>();
            if (!v) continue;

            vcams[k] = v;
            ApplyFollowLookAt(v);
        }
    }

    public void DebugPrintRegisteredKeys()
    {
        var keys = string.Join(", ", vcams.Keys);
        Debug.Log($"[CameraManager] Registered keys: [{keys}]");
    }
}
