using UnityEngine;
using UnityEngine.Playables;

public class CutsceneManager : MonoBehaviour
{
    [Header("Director")]
    [SerializeField] PlayableDirector director;

    [Header("Scene Refs (found at runtime)")]
    NPC_CutsceneActor npc;
    Transform player;

    [Header("Cutscene Points (FarmScene objects)")]
    [SerializeField] Transform npcEnterPoint;   // NPC 시작 위치(선택)
    [SerializeField] Transform npcExitPoint;    // 퇴장 목표 위치
    [SerializeField] Transform talkStandPoint;  // "플레이어 앞"에 서게 할 위치(추천: 빈 오브젝트)

    [Header("Dialogue")]
    [SerializeField] string dialogueId = "NPC_INTRO_001";

    void Awake()
    {
        if (!director) director = GetComponent<PlayableDirector>();
    }

    public void PlayCutscene()
    {
        ResolveRefs();
        if (!director) return;

        director.time = 0;
        director.Play();
    }

    void ResolveRefs()
    {
        // Additive라면 호출 시점에 항상 다시 찾는 게 안전
        if (!npc) npc = FindObjectOfType<NPC_CutsceneActor>(true);
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    // ====== Signal에서 호출될 함수들 ======

    // 1) NPC가 플레이어 앞으로 걸어오기
    public void Sig_NPC_Enter()
    {
        ResolveRefs();
        if (!npc || !player) return;

        // talkStandPoint가 있으면 그곳으로, 없으면 플레이어 기준으로 앞(예: 아래쪽) 계산
        Vector3 target = talkStandPoint
            ? talkStandPoint.position
            : player.position + new Vector3(0f, -1f, 0f);

        // 필요하면 등장 위치 강제
        if (npcEnterPoint) npc.transform.position = npcEnterPoint.position;

        npc.MoveTo(target);
    }

    // 2) 대화 시작 + Timeline Pause
    public void Sig_Talk_Start()
    {
        ResolveRefs();
        if (!npc || !player || !director) return;

        npc.Stop();
        npc.FaceTo(player.position);

        // 타임라인 정지(대화 끝날 때 Resume)
        director.Pause();

        // ===== 너의 대화 시스템 연결 부분 =====
        // 아래는 예시야. 네 프로젝트 대화 매니저 호출로 바꿔줘.
        // "대화 종료 이벤트"에서 반드시 OnDialogueEnded가 호출되게만 하면 끝.

        //DialogueManager.Instance.StartDialogue(dialogueId, onFinished: OnDialogueEnded);
    }

    void OnDialogueEnded()
    {
        // 대화가 끝나면 타임라인 다시 진행 -> Exit 신호까지 재생됨
        if (director) director.Resume();
    }

    // 3) 퇴장 시작
    public void Sig_NPC_Exit()
    {
        ResolveRefs();
        if (!npc) return;

        Vector3 target = npcExitPoint ? npcExitPoint.position : npc.transform.position + new Vector3(3f, 0f, 0f);
        npc.MoveTo(target);
    }

    // 4) 컷신 끝 정리(선택)
    public void Sig_Cutscene_End()
    {
        ResolveRefs();
        if (npc) npc.Stop();
        // PlayerActionLock.Unlock("Cutscene") 같은 것도 여기서
    }
}
