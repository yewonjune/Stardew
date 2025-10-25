using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCScheduleHolder : MonoBehaviour
{
    [System.Serializable]
    public class Schedule
    {
        public int hour;
        public int minute;
        public Transform target;
    }

    public Schedule[] schedules;
}
