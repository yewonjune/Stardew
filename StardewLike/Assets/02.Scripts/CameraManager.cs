using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    public Cinemachine.CinemachineVirtualCamera mainCam;
    public Cinemachine.CinemachineVirtualCamera houseCam;

    public Transform player;

    void Awake()
    {
        Instance = this;
    }

    public void SwitchToFarm()
    {
        mainCam.Priority = 11;
        houseCam.Priority = 0;
    }

    public void SwitchToHouse()
    {
        mainCam.Priority = 0;
        houseCam.Priority = 11;
    }

}
