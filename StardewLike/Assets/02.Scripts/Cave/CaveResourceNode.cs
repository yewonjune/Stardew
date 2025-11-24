using UnityEngine;

public class CaveResourceNode : MonoBehaviour
{
    public ResourceType resourceType;
    public ToolType requiredTool;
    public int maxHp = 3;
    public int minPower = 1;
    int hp;
    bool isBroken = false;

    [Header("Drops")]
    public Item dropItem;
    public GameObject dropPrefab;
    public int dropCount = 1;
    public float scatterRadius = 0.2f;

    [Header("Ladder")]
    public bool canSpawnLadder = true;
    public float ladderChance = 0.05f;
    public GameObject ladderPrefab;

    void Awake()
    {
        hp = maxHp;
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

    void Break()
    {
        TrySpawnLadder();

        SpawnDrops();

        Destroy(gameObject);
    }

    void SpawnDrops()
    {
        if (!dropPrefab) return;

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

    void TrySpawnLadder()
    {
        if (!ladderPrefab) return;

        bool isLastRock = IsLastRockInThisVisit();

        bool hasLadderAlready = false;
        var ladders = FindObjectsOfType<CaveLadder>();
        foreach (var ladder in ladders)
        {
            if (ladder.gameObject.scene == gameObject.scene)
            {
                hasLadderAlready = true;
                break;
            }
        }

        bool shouldSpawn = false;

        if (isLastRock && !hasLadderAlready)
        {
            shouldSpawn = true;
        }
        else
        {
            shouldSpawn = Random.value < ladderChance;
        }

        if (!shouldSpawn) return;

        Transform parent = transform.parent;
        Instantiate(ladderPrefab, transform.position, Quaternion.identity);
    }


    bool IsLastRockInThisVisit()
    {
        var all = FindObjectsOfType<CaveResourceNode>();
        int others = 0;
        foreach (var n in all)
        {
            if (n == this) continue;
            if (!n.canSpawnLadder) continue;
            if (n.gameObject.scene != gameObject.scene) continue;
            others++;
        }
        return others == 0;
    }
}
