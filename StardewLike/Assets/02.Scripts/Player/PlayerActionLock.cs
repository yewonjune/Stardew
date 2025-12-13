using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerActionLock
{
    static int lockCount;
    public static bool IsLocked => lockCount > 0;

    public static void Lock(string reason = "")
    {
        lockCount++;
    }

    public static void Unlock(string reason = "")
    {
        lockCount = Mathf.Max(0, lockCount - 1);
    }

    public static void ResetAll()
    {
        lockCount = 0;
    }
}
