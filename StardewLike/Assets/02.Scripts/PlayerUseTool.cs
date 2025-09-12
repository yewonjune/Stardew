using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUseTool : MonoBehaviour
{
    public HotbarManager hotbarManager;
    public Transform useToolPoint;

    [SerializeField] PlayerMovement playerMovement;

    [SerializeField] float hitRadius = 0.15f;                 // 칸 판정 여유
    [SerializeField] LayerMask resourceLayer = ~0;            // 자원만 맞추고 싶으면 레이어 지정

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Item selectedItem = hotbarManager.GetSelectedItem();

            if (selectedItem is Tools tool)
            {
                UseTool(tool);
            }
        }
    }

    void UseTool(Tools tool)
    {
        Debug.Log($"사용 중: {tool.itemName} ({tool.toolType})");

        switch (tool.toolType)
        {
            case ToolType.Hoe:
                // 땅 파기
                DigSoilWithHoe();
                break;

            case ToolType.Pickaxe:      // 돌 깨기

            case ToolType.Axe:          // 나무 베기
            case ToolType.Scythe:       // 작물 베기(?)
                BreakResourceWithTool(tool);
                break;

            case ToolType.WateringCan:
                // 물 뿌리기
                WaterCropWithWateringCan();
                break;

            case ToolType.Sword:
                // 공격
                AttackWithSword();
                break;

            case ToolType.Fishingrod:
                //낚시
                FishingWithFishingrod();
                break;
        }
    }

    void DigSoilWithHoe()
    {
        SoilTilemapController soil = FindObjectOfType<SoilTilemapController>();
        if (soil == null)
        {
            Debug.LogWarning("[Hoe] SoilTilemapController가 씬에 없음");
            return;
        }

        Vector3Int playerCell = soil.groundTilemap.WorldToCell(transform.position);

        // 4방 기준으로 바라보는 방향 결정
        Vector2 d = (playerMovement != null && playerMovement.lastDirection.sqrMagnitude > 0.0001f)
                    ? playerMovement.lastDirection
                    : Vector2.down;

        Vector3Int offset;
        if (Mathf.Abs(d.x) >= Mathf.Abs(d.y))
            offset = (d.x >= 0) ? Vector3Int.right : Vector3Int.left;
        else
            offset = (d.y >= 0) ? Vector3Int.up : Vector3Int.down;

        Vector3Int targetCell = playerCell + offset;
        Vector3 center = soil.groundTilemap.GetCellCenterWorld(targetCell);

        bool ok = soil.TryTillAtWorldPos(center);
        Debug.Log(ok ? "[Hoe] 땅 갈기 성공" : "[Hoe] 갈 수 없는 위치");
    }

    //void BreakRockWithPickaxe()
    //{
    //    RaycastHit2D hit = Physics2D.Raycast(useToolPoint.position, Vector2.zero);

    //    if (hit.collider != null)
    //    {
    //        Debug.Log("돌깨기");
    //    }
    //}

    //void ChopTreeWithAxe()
    //{
    //    RaycastHit2D hit = Physics2D.Raycast(useToolPoint.position, Vector2.zero);

    //    if (hit.collider != null)
    //    {
    //        Debug.Log("나무베기");
    //    }
    //}

    void WaterCropWithWateringCan()
    {
        RaycastHit2D hit = Physics2D.Raycast(useToolPoint.position, Vector2.zero);

        if (hit.collider != null)
        {
            Debug.Log("물뿌리기");
        }
    }

    //void HarvestCropWithScythe()
    //{
    //    RaycastHit2D hit = Physics2D.Raycast(useToolPoint.position, Vector2.zero);

    //    if (hit.collider != null)
    //    {
    //        Debug.Log("작물 베기");
    //    }
    //}

    void AttackWithSword()
    {
        RaycastHit2D hit = Physics2D.Raycast(useToolPoint.position, Vector2.zero);

        if (hit.collider != null)
        {
            Debug.Log("몬스터 공격하기");
        }
    }
    void FishingWithFishingrod()
    {
        PlayerFishingController fishingController = GetComponent<PlayerFishingController>();
        if (fishingController == null)
        {
            Debug.LogWarning("PlayerFishingController가 없음! 낚시 불가능.");
            return;
        }

        if (fishingController.isFishing)
            fishingController.TryStopFishing();
        else
            fishingController.TryStartFishing();
    }

    void BreakResourceWithTool(Tools tool)
    {
        Collider2D collider = Physics2D.OverlapCircle(useToolPoint.position, hitRadius, resourceLayer);
        if (collider != null)
        {
            ResourceNode resourceNode = collider.GetComponent<ResourceNode>();
            if (resourceNode != null)
            {
                resourceNode.Hit(tool);
                return;
            }
        }
        Debug.Log("[Tool] 맞출 자원이 없음");
    }

    void OnDrawGizmosSelected()
    {
        if (useToolPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(useToolPoint.position, hitRadius);
        }
    }
}
