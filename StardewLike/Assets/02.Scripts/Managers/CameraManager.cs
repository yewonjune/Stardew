using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    public Cinemachine.CinemachineVirtualCamera mainCam;
    public Cinemachine.CinemachineVirtualCamera houseCam;
    public Cinemachine.CinemachineVirtualCamera villageCam;

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
    public void SwitchToVillage()
    {
        mainCam.Priority = 0;
        villageCam.Priority = 11;
    }
}
