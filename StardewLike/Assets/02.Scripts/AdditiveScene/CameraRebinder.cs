using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRebinder : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // 플레이어 Transform
        var player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (!player) return;

        // 씬에 있는 모든 VCAM에 대해 Follow 재설정
        var vcams = FindObjectsOfType<CinemachineVirtualCamera>(includeInactive: true);
        foreach (var vcam in vcams)
        {
            if (vcam && vcam.Follow == null)
                vcam.Follow = player;

            // 각 VCAM의 Confiner2D 캐시만 무효화 (BoundCollider는 인스펙터에 이미 연결되어 있음)
            var conf = vcam.GetComponent<CinemachineConfiner2D>();
            if (conf) conf.InvalidateCache();
        }
    }
    
}
