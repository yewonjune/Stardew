using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    public ResourceType resourceType;
    public int maxHp = 3;
    int hp;

    public ToolType requiredTool;
    public int minPower = 1;

    [Header("Drop Settings")]
    public Item dropItem;                 // 인벤토리용 데이터(ScriptableObject)
    public GameObject dropPrefab;         // 씬에 떨어질 프리팹(예: StoneDrop.prefab)
    public int dropCount = 1;             // 몇 개 떨어뜨릴지
    public float scatterRadius = 0.2f;    // 살짝 흩뿌리기

    public string prefabId;

    bool isBroken = false;

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

        if (!st.resources.Exists(r => r.prefabId == prefabId && (r.position - transform.position).sqrMagnitude < 0.0001f))
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

        hp -= Mathf.Max(1, tool.power);
        Debug.Log($"[Resource] {resourceType} 맞음! 남은 HP = {hp}");

        if (hp <= 0)
        {
            isBroken = true;
            Break();
        }
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
