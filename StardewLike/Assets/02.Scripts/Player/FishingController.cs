using System.Collections;
using UnityEngine;

public class PlayerFishingController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Animator animator;          // Player Animator
    [SerializeField] Transform feet;             // 발 위치 (없으면 transform 사용)

    [Header("Input")]
    [SerializeField] KeyCode fishKey = KeyCode.E;

    [Header("Water Detect")]
    [SerializeField] LayerMask fishingZoneLayer;       // Water 레이어 체크
    [SerializeField] float detectRadius = 0.9f;  // 감지 반경

    [Header("Timing")]
    [SerializeField] float startToLoopDelay = 0.5f;                // Start → Loop 전환 대기

    public bool isFishing;
    public bool inLoopPhase;

    readonly Collider2D[] _hits = new Collider2D[4];

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!feet) feet = transform;

        if (fishingZoneLayer.value == 0)
            fishingZoneLayer = LayerMask.GetMask("FishingZone");
    }

    public void TryStartFishing()
    {
        if (isFishing) return;
        if (!IsInFishingZone()) return;

        StartCoroutine(FishRoutine());
    }

    public void TryStopFishing()
    {
        if (!isFishing) return;

        StopFishing();
    }

    bool IsInFishingZone()
    {
        Vector2 pos = feet ? (Vector2)feet.position : (Vector2)transform.position;
        int count = Physics2D.OverlapCircleNonAlloc(pos, detectRadius, _hits, fishingZoneLayer);
        return count > 0;
    }

    IEnumerator FishRoutine()
    {
        isFishing = true;
        inLoopPhase = false;

        animator.SetTrigger("StartFishing");
        animator.SetBool("IsFishing", true);

        yield return new WaitForSeconds(startToLoopDelay);

        inLoopPhase = true;
    }

    void StopFishing()
    {
        inLoopPhase = false;

        animator.SetTrigger("StopFishing");
        animator.SetBool("IsFishing", false);

        isFishing = false;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 p = feet ? feet.position : transform.position;
        Gizmos.DrawWireSphere(p, detectRadius);
    }
}
