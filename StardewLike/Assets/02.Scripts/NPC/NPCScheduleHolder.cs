using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Schedule
{
    public int hour;
    public int minute;
    public Transform[] path;
}

public class NPCScheduleHolder : MonoBehaviour
{
    public NPCMovement movement;
    public Schedule[] schedules;
    NPCScheduleManager mgr;

    [HideInInspector] public int lastAppliedBestTime = -1;

    void OnEnable()
    {
        mgr = FindObjectOfType<NPCScheduleManager>();
        if (mgr != null) mgr.Register(this);
    }

    void OnDisable()
    {
        if (mgr != null) mgr.Unregister(this);
    }
}
