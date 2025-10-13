using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GamePause
{
    static int count;
    public static bool isPaused => count > 0;

    public static void Pause()
    {
        count = 0;
        Time.timeScale = 0;
    }

    public static void Resume()
    {
        count = Mathf.Max(0, count - 1);
        if (count == 0) Time.timeScale = 1f;
    }
    public static void ResetAll()
    {
        count = 0;
        Time.timeScale = 1f;
    }
}
