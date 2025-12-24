using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

[CreateAssetMenu(menuName = "Cutscene/CutsceneCatalog")]
public class CutsceneCatalog : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string id;
        public TimelineAsset timeline;
    }

    public Entry[] entries;

    public TimelineAsset Get(string id)
    {
        if (entries == null) return null;
        foreach (Entry e in entries) 
            if(e!=null && e.id==id) return e.timeline;
        return null;
    }
}
