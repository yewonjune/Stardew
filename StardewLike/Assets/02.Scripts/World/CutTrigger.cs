using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutTrigger : MonoBehaviour
{
    [SerializeField] string cutsceneId;
    [SerializeField] bool once = true;
    [SerializeField] CutsceneManager cutsceneManager;

    bool fired;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if(once && fired) return;

        fired = true;

        if (!cutsceneManager)
            cutsceneManager = FindObjectOfType<CutsceneManager>(true);

        if (cutsceneManager)
            cutsceneManager.PlayCutscene();
        else
            Debug.LogError("[CutTrigger] CutsceneManager not found");
    }
}
