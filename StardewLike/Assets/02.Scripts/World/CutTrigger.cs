using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutTrigger : MonoBehaviour
{
    [SerializeField] string cutsceneId;
    [SerializeField] bool once = true;

    bool fired;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if(once && fired) return;

        fired = true;
        //CutsceneManager..TryPlay(cutsceneId);
    }
}
