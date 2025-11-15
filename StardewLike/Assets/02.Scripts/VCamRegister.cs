using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VCamRegister : MonoBehaviour
{
    public string key;

    CinemachineVirtualCamera vcam;

    void Awake() => vcam = GetComponent<CinemachineVirtualCamera>();

    void OnEnable()
    {
        if (CameraManager.Instance && !string.IsNullOrWhiteSpace(key))
            CameraManager.Instance.RegisterVCam(key, vcam);
    }

    void OnDisable()
    {
        if (CameraManager.Instance && !string.IsNullOrWhiteSpace(key))
            CameraManager.Instance.UnregisterVCam(key, vcam);
    }
}
