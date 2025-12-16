using UnityEngine;
using DG.Tweening;

public class DropItemController : MonoBehaviour
{
    public bool useJumpTween = true;
    public float jumpPower = 1.2f;
    public float jumpDuration = 0.35f;
    public Vector2 randomSpawnOffset = new Vector2(0.5f, 0f);

    public bool enableAutoCollect = true;
    public float collectEnableDelay = 0.5f;

    public float detectRadius = 3.0f;

    public float followRadius = 1.3f;
    public float followSpeed = 7f;

    Rigidbody2D rb;
    Transform player;
    bool canCollect = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;
    }

    void Start()
    {
        PlaySpawnMotion();
        if (enableAutoCollect)
            Invoke(nameof(EnableCollect), collectEnableDelay);
    }

    void Update()
    {
        if (!enableAutoCollect) return;
        if (!canCollect) return;
        if (!player) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist > detectRadius)
        {
            return; // °¡¸¸È÷
        }

        if (dist > followRadius)
        {
            return;
        }

        if (rb) rb.simulated = false;
        transform.position = Vector3.MoveTowards(transform.position, player.position, followSpeed * Time.deltaTime);
    }

    void PlaySpawnMotion()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + new Vector3(
            Random.Range(-randomSpawnOffset.x, randomSpawnOffset.x),
            Random.Range(0f, randomSpawnOffset.y),
            0f
        );

        if (useJumpTween)
        {
            transform.position = startPos;
            transform.DOJump(targetPos, jumpPower, 1, jumpDuration)
                     .SetEase(Ease.OutQuad);
        }
    }

    void EnableCollect()
    {
        canCollect = true;
    }

    public void OnCollectFailed()
    {
        canCollect = false;
        enableAutoCollect = false;

        if (rb) rb.simulated = true;
    }
}
