using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CaveStateManager
{
    public static int CurrentCaveIndex = -1;
    public static int CurrentFloor = 1;

    public static void ResetToEntrance()
    {
        CurrentCaveIndex = 0;
        CurrentFloor = 1;
    }
}
