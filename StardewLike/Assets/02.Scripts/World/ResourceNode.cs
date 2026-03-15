using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ResourceNode : MonoBehaviour
{
    public ResourceType resourceType;
    public int maxHp = 3;
    int hp;

    public ToolType requiredTool;
    public int minPower = 1;

    [Header("Drop Settings")]
    public Item dropItem;
    public GameObject dropPrefab;
    public int dropCount = 1;
    public float scatterRadius = 0.2f;    // 살짝 흩뿌리기

    public float shakeDuration = 0.15f;   // 흔들리는 시간
    public float shakeStrength = 0.1f;    // 흔들림 세기
    public int shakeVibrato = 12;         // 흔들리는 횟수

    public float punchScale = 0.15f;      // 살짝 '쿡' 눌리는 크기
    public float punchDuration = 0.2f;    // 스케일 튀는 시간

    public string prefabId;

    bool isBroken = false;

    [HideInInspector] public bool isRestoredFromSave = false;

    private void Awake()
    {
        hp = maxHp;

        if (string.IsNullOrEmpty(prefabId))
        {
            prefabId = gameObject.name.Replace("(Clone)", "").Trim();
        }
    }
    void Start()
    {
        WorldStateManager worldStateManager = WorldStateManager.Instance;
        if (worldStateManager == null)
        {
            Debug.LogWarning("[Soil] WorldStateManager.Instance 가 없습니다. 복원을 생략합니다.");
            return;
        }

        var st = worldStateManager.GetOrCreate(gameObject.scene.name);

        if (isRestoredFromSave) return;

        bool alreadyRegistered = st.resources.Exists(r => r.prefabId == prefabId &&
                                                         (r.position - transform.position).sqrMagnitude < 0.0001f);

        if (!alreadyRegistered)
        {
            worldStateManager.AddResource(gameObject.scene.name, new ResourceSave
            {
                prefabId = prefabId,
                position = transform.position,
                harvestedOrRemoved = false
            });
        }
    }

    public void Hit(Tools tool)
    {
        if (isBroken) return;

        if (tool.toolType != requiredTool)
        {
            Debug.Log($"[Resource] {resourceType} 는 {tool.toolType} 으로 채굴 불가");
            return;
        }

        if (tool.power < minPower)
        {
            Debug.Log($"[Resource] {resourceType} 은 최소 {minPower} 파워 필요");
            return;
        }

        transform.DOKill();

        hp -= Mathf.Max(1, tool.power);
        Debug.Log($"[Resource] {resourceType} 맞음! 남은 HP = {hp}");


        if (punchScale > 0f)
        {
            transform.DOPunchScale(
                new Vector3(punchScale, punchScale, 0f),
                punchDuration,
                8,      // 진동 횟수
                1f      // 탄성
            );
        }

        transform.DOShakePosition(
                    shakeDuration,
                    shakeStrength,
                    shakeVibrato,
                    90f,
                    false,
                    true
                )
                .OnComplete(() =>
                {
                    //HP 확인 후 파괴
                    if (hp <= 0 && !isBroken)
                    {
                        isBroken = true;
                        Break();
                    }
                });
    }
    public void Harvest()
    {
        Break();
    }

    void Break()
    {
        SpawnDrops();

        WorldStateManager worldStateManager = WorldStateManager.Instance;
        if (worldStateManager != null)
        {
            string sceneName = gameObject.scene.name;
            worldStateManager.MarkResourceRemoved(sceneName, transform.position);
        }
        else
        {
            Debug.LogWarning("[Resource] WorldStateManager.Instance 가 null 입니다. 파괴 상태가 저장되지 않습니다.");
        }

        Destroy(gameObject);
    }

    void SpawnDrops()
    {
        if (dropPrefab == null)
        {
            Debug.LogWarning("[Resource] dropPrefab이 비어있어 드랍을 생성할 수 없습니다.");
            return;
        }
        else
        {
            for (int i = 0; i < dropCount; i++)
            {
                Vector2 offset = Random.insideUnitCircle * scatterRadius;
                var go = Instantiate(dropPrefab, transform.position + (Vector3)offset, Quaternion.identity);

                var pickup = go.GetComponent<ItemPickup>();
                if (pickup != null && dropItem != null)
                {
                    pickup.itemData = dropItem;
                }
            }
        }
    }

}
