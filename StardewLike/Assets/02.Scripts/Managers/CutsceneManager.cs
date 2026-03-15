using UnityEngine;
using UnityEngine.Playables;

public class CutsceneManager : MonoBehaviour
{
    [SerializeField] PlayableDirector director;
    [SerializeField] DialogueData dialogueData;      // 3줄짜리 대사
    [SerializeField] GameObject npcCutsceneRoot;
    [SerializeField] string cutsceneId;

    bool playing;

    void Awake()
    {
        if (!director) director = FindObjectOfType<PlayableDirector>(true);

        if (npcCutsceneRoot && !string.IsNullOrEmpty(cutsceneId) && CutsceneRecord.HasFired(cutsceneId))
            npcCutsceneRoot.SetActive(false);
    }

    public void PlayCutscene()
    {
        if (playing) return;
        playing = true;

        PlayerActionLock.Lock("Cutscene");

        if (director)
        {
            director.time = 0;
            director.Play();
        }
    }

    // === Timeline Signal에서 호출 ===
    public void Sig_Talk_Start()
    {
        if (!director) return;

        director.Pause();

        if (DialogueManager.Instance == null || dialogueData == null)
        {
            director.Resume();
            return;
        }

        DialogueManager.Instance.StartDialogue(dialogueData, () =>
        {
            if (director) director.Resume();
        });
    }

    // === Timeline 마지막 Signal에서 호출(선택) ===
    public void Sig_Cutscene_End()
    {
        Debug.Log("[Cutscene] Sig_Cutscene_End -> Unlock");
        PlayerActionLock.Unlock("Cutscene");
        playing = false;

        // NPC를 씬에서 사라지게 하고 싶으면
        if (npcCutsceneRoot) npcCutsceneRoot.SetActive(false);
    }
}
