using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class VCamAutoBind : MonoBehaviour
{
    CinemachineVirtualCamera vcam;

    void Awake() => vcam = GetComponent<CinemachineVirtualCamera>();

    void OnEnable()
    {
        if (!vcam) return;

        // Follow가 비었거나 무효(프리팹/다른 씬)이면 보정
        if (!vcam.Follow || !vcam.Follow.gameObject.scene.IsValid())
        {
            var player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player) vcam.Follow = player;
        }

        // Confiner2D 캐시 갱신 (바운더리는 인스펙터에 이미 연결되어 있다고 가정)
        vcam.GetComponent<CinemachineConfiner2D>()?.InvalidateCache();
    }
}
