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
}
