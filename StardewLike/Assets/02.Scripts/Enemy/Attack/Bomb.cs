using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public int damage = 1;

    [Header("Timing")]
    public float fuseShortenDelay = 0.5f;   // Idle → FuseShort까지 대기 시간
    public float preFlashDelay = 0.7f;      // FuseShort → PreFlash까지 대기
    public float explodeDelay = 0.4f;       // PreFlash → Explode까지 대기

    Animator animator;
    bool exploded = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        // 생성되면 바로 타이머 시작
        StartCoroutine(FuseRoutine());
    }

    IEnumerator FuseRoutine()
    {
        // 1. Idle 상태로 있다가
        yield return new WaitForSeconds(fuseShortenDelay);

        // 2. 불씨 줄어드는 애니메이션
        animator.SetTrigger("Shorten");
        yield return new WaitForSeconds(preFlashDelay);

        // 3. 반짝반짝 (3번 깜빡이는 클립)
        animator.SetTrigger("PreFlash");
        yield return new WaitForSeconds(explodeDelay);

        // 4. 폭발
        Explode();
    }

    void Explode()
    {
        if (exploded) return;
        exploded = true;

        // 폭발 애니메이션 재생
        animator.SetTrigger("Explode");

        // 여기서 주변에 데미지 주고 싶으면 OverlapCircle 등 사용
        // Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.5f, playerLayer);
        // ...

        // 실제 오브젝트 삭제는 애니메이션 이벤트에서 호출
        // (마지막 프레임에서 BombBehavior.OnExplosionEnd 호출)
    }

    // 애니메이션 마지막 프레임에서 이벤트로 호출
    public void OnExplosionEnd()
    {
        Destroy(gameObject);
    }
}
