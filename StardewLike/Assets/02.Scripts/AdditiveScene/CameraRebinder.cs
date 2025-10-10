using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRebinder : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (!player) return;

        var vcams = FindObjectsOfType<CinemachineVirtualCamera>(includeInactive: true);
        foreach (var vcam in vcams)
        {
            if (vcam && vcam.Follow == null)
                vcam.Follow = player;

            var conf = vcam.GetComponent<CinemachineConfiner2D>();
            if (conf) conf.InvalidateCache();
        }
    }
    
}
