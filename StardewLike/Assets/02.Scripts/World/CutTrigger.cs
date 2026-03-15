using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutTrigger : MonoBehaviour
{
    [SerializeField] string cutsceneId;
    [SerializeField] bool once = true;
    [SerializeField] CutsceneManager cutsceneManager;

    void Awake()
    {
        if (once && HasFired())
            gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if(once && HasFired()) return;

        if (string.IsNullOrEmpty(cutsceneId))
            Debug.LogWarning("[CutTrigger] cutsceneId가 비어 있습니다.");

        if (!cutsceneManager)
            cutsceneManager = FindObjectOfType<CutsceneManager>(true);

        if (cutsceneManager)
        {
            if (once && !string.IsNullOrEmpty(cutsceneId))
                CutsceneRecord.MarkFired(cutsceneId);

            cutsceneManager.PlayCutscene();

            if (once) gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("[CutTrigger] CutsceneManager not found");
        }
    }

    bool HasFired() => !string.IsNullOrEmpty(cutsceneId) && CutsceneRecord.HasFired(cutsceneId);
}
