using System.Collections;
using System.Collections.Generic;

public static class CutsceneRecord
{
    static readonly HashSet<string> fired = new();

    public static bool HasFired(string id) => fired.Contains(id);

    public static void MarkFired(string id) => fired.Add(id);

    public static string[] GetAll() => new List<string>(fired).ToArray();

    public static void ReplaceAll(string[] ids)
    {
        fired.Clear();
        if (ids == null) return;
        foreach (var id in ids)
            if (!string.IsNullOrEmpty(id)) fired.Add(id);
    }
}
