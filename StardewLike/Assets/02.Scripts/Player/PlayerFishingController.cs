using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerFishingController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] Transform feet;

    [SerializeField] LayerMask fishingZoneLayer;
    [SerializeField] float detectRadius = 0.9f;

    [SerializeField] float startToLoopDelay = 0.5f;

    [SerializeField] Vector2 biteTimeRange = new Vector2(2f, 5f); // ! 뜨기까지
    [SerializeField] float reactionTime = 2.0f;                   // ! 동안 클릭 가능 시간
    [SerializeField] GameObject exclamationMark;                  // ! 아이콘 (월드 스페이스 UI 등)
    [SerializeField] Vector3 markOffset = new Vector3(0f, 1.6f, 0f);

    [SerializeField] FishCatalog fishCatalog;                     // 캔버스에 올려둔 SO
    public UnityEvent<Item, int> OnFishCaught = new UnityEvent<Item, int>();

    public bool isFishing;
    public bool inLoopPhase;

    readonly Collider2D[] _hits = new Collider2D[4];

    Coroutine fishingCo;
    bool biteReady;
    bool caught;

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!feet) feet = transform;

        if (fishingZoneLayer.value == 0)
            fishingZoneLayer = LayerMask.GetMask("FishingZone");

        HideMark();
    }

    void Update()
    {
        if (isFishing && biteReady && Input.GetMouseButtonDown(0))
        {
            TryCatch();
            Debug.Log("[Fishing] TryCatch()");
        }
    }
    void LateUpdate()
    {
        if (isFishing && biteReady && exclamationMark && exclamationMark.activeSelf)
        {
            Vector3 basePos = feet ? feet.position : transform.position;
            exclamationMark.transform.position = basePos + markOffset;
        }
    }

    public void TryStartFishing()
    {
        if (isFishing) return;
        if (!IsInFishingZone()) return;

        if (fishingCo != null) StopCoroutine(fishingCo);
        fishingCo = StartCoroutine(FishRoutine());
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
        biteReady = false;
        caught = false;

        animator.SetTrigger("StartFishing");
        animator.SetBool("IsFishing", true);

        yield return new WaitForSeconds(startToLoopDelay);

        inLoopPhase = true;

        float wait = Random.Range(biteTimeRange.x, biteTimeRange.y);
        yield return new WaitForSeconds(wait);

        ShowMark();
        biteReady = true;

        float t = 0f;
        while (t < reactionTime && biteReady && !caught)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (!caught)
        {
            biteReady = false;
            HideMark();
        }

        StopFishing();

    }

    void TryCatch()
    {
        if (!biteReady) return;

        biteReady = false;
        caught = true;
        HideMark();

        FishData fish = fishCatalog ? fishCatalog.PickRandomFish(reactionTime) : null;
        if (fish == null || fish.item == null)
        {
            Debug.LogWarning("FishCatalog에 유효한 FishData/Item이 없습니다.");
            return;
        }

        // 개수 규칙: 기본 1개
        int count = 1;
        OnFishCaught?.Invoke(fish.item, count);

        // 디버그: 크기/가격 참고
        int size = Random.Range(fish.sizeRange.x, fish.sizeRange.y + 1);
        Debug.Log($"잡은 물고기: {fish.item.itemName}, 크기: {size}cm, 예상가: {fish.basePrice}");
    }

    void StopFishing()
    {
        inLoopPhase = false;

        animator.SetTrigger("StopFishing");
        animator.SetBool("IsFishing", false);

        biteReady = false;
        HideMark();

        isFishing = false;

        if (fishingCo != null)
        {
            StopCoroutine(fishingCo);
            fishingCo = null;
        }
    }
    void ShowMark()
    {
        if (!exclamationMark) return;
        exclamationMark.SetActive(true);

        Vector3 basePos = feet ? feet.position : transform.position;
        exclamationMark.transform.position = basePos + markOffset;

        exclamationMark.transform.localScale = Vector3.one * 1.15f;
        Debug.Log("[Fishing] ! shown → biteReady=true");
    }

    void HideMark()
    {
        if (exclamationMark) exclamationMark.SetActive(false);
    }
    void OnDrawGizmosSelected()
    {
        Vector3 p = feet ? feet.position : transform.position;
        Gizmos.DrawWireSphere(p, detectRadius);
    }
}
