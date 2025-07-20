using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    public Cinemachine.CinemachineVirtualCamera mainCam;
    public CinemachineConfiner2D confiner2D;
    public Transform player;

    void Awake()
    {
        Instance = this;
    }
    public void FollowPlayer()
    {
        mainCam.Follow = player;
        mainCam.LookAt = player;

        if (confiner2D != null)
        {
            confiner2D.enabled = true; // 제한 다시 활성화
        }
    
    }

    public void MoveToStaticView(Transform view)
    {
        mainCam.Follow = null;
        mainCam.LookAt = null;

        // 제한 끄기
        if (confiner2D != null)
        {
            confiner2D.enabled = false;
        }

        mainCam.transform.position = view.position;
        mainCam.transform.rotation = view.rotation;
    }

}
