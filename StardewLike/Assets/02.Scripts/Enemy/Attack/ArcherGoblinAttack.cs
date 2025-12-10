using UnityEngine;

public class ArcherGoblinAttack : MonoBehaviour
{
    public GameObject arrowPrefab;
    public Transform shootPoint;
    public float arrowSpeed = 6f;
    public int arrowDamage = 1;
    public float arrowLifeTime = 5f;

    public void ShootArrow()
    {
        if (arrowPrefab == null || shootPoint == null) return;

        GameObject arrow = Instantiate(arrowPrefab, shootPoint.position, Quaternion.identity);

        Destroy(arrow, arrowLifeTime);

        ArrowBehavior behavior = arrow.AddComponent<ArrowBehavior>();
        behavior.damage = arrowDamage;
        behavior.speed = arrowSpeed;
        behavior.shooter = this.gameObject;
    }
}

public class ArrowBehavior : MonoBehaviour
{
    public int damage;
    public float speed;
    public GameObject shooter;

    private Vector2 moveDir;

    void Start()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player != null)
        {
            Vector2 toPlayer = (player.position - transform.position);

            if (toPlayer.sqrMagnitude > 0.0001f)
            {
                // 가장 가까운 축으로 스냅 (상/하/좌/우)
                if (Mathf.Abs(toPlayer.x) >= Mathf.Abs(toPlayer.y))
                {
                    // 좌우
                    moveDir = toPlayer.x >= 0 ? Vector2.right : Vector2.left;
                }
                else
                {
                    // 상하
                    moveDir = toPlayer.y >= 0 ? Vector2.up : Vector2.down;
                }
            }
        }

        float angle = 0f;

        if (moveDir == Vector2.down) angle = 0f;
        else if (moveDir == Vector2.left) angle = -90f;
        else if (moveDir == Vector2.right) angle = 90f;
        else if (moveDir == Vector2.up) angle = 180f;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Update()
    {
        transform.Translate(moveDir * speed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 자신한테 부딪혀서 데미지 들어가는 거 방지
        if (collision.gameObject == shooter) return;

        if (collision.CompareTag("Player"))
        {
            // PlayerDamageController 같은 곳에 데미지 전달
            //collision.GetComponent<PlayerHealth>()?.TakeDamage(damage);

            Destroy(gameObject);
        }

        //if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        //{
        //    Destroy(gameObject);
        //}
    }
}
