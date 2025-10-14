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

        if (!vcam.Follow || !vcam.Follow.gameObject.scene.IsValid())
        {
            var player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player) vcam.Follow = player;
        }

        vcam.GetComponent<CinemachineConfiner2D>()?.InvalidateCache();
    }
}
