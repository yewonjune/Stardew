using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerUseTool : MonoBehaviour
{
    public HotbarManager hotbarManager;
    public Transform useToolPoint;

    public Animator animator;

    [SerializeField] PlayerMovement playerMovement;

    [SerializeField] SoilTilemapController soilTilemapController;
    [SerializeField] Inventory inventory;

    [SerializeField] bool useMouseTarget = false;

    [SerializeField] float hitRadius = 0.15f;                 // 칸 판정 여유
    [SerializeField] LayerMask resourceLayer = ~0;            // 자원만 맞추고 싶으면 레이어 지정

    const string ParamToolIndex = "ToolIndex";
    const string TrigStartTool = "StartAction_Tool";
    const string StateToolAction = "PlayerAction";

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        RebindSoilController(); // 현재 이미 로드되어 있으면 즉시도 시도
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 새 씬이 활성화되면 그 씬의 SoilTilemapController로 갈아끼움
        if (scene == SceneManager.GetActiveScene())
            RebindSoilController();
    }

    void RebindSoilController()
    {
        soilTilemapController = null;

        // 활성 씬에 있는 것만 선택 (Additive 로드 시 다수 방지)
        var all = FindObjectsOfType<SoilTilemapController>(includeInactive: false);
        var activeScene = SceneManager.GetActiveScene();
        foreach (var s in all)
        {
            if (s.gameObject.scene == activeScene)
            {
                soilTilemapController = s;
                break;
            }
        }

        if (!soilTilemapController)
            Debug.Log("[PlayerUseTool] 활성 씬에 SoilTilemapController 없음 (마을/집일 수 있음)");
    }

    // Update is called once per frame
    void Update()
    {
        UpdateUseToolPoint();

        if (Input.GetMouseButtonDown(0))
        {
            Item selectedItem = hotbarManager.GetSelectedItem();

            if (selectedItem is Tools tool)
            {
                UseTool(tool);
            }
            else if (selectedItem is Seeds seed)
            {
                TryPlantSeed(seed);
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryHarvestAtTarget();
        }
    }

    void UseTool(Tools tool)
    {
        Debug.Log($"사용 중: {tool.itemName} ({tool.toolType})");

        switch (tool.toolType)
        {
            case ToolType.Hoe:
              
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
        if (!soilTilemapController) { Debug.LogWarning("[Hoe] SoilTilemapController가 없음"); return; }

        Vector3Int playerCell = soilTilemapController.groundTilemap.WorldToCell(transform.position);

        Vector2 d = (playerMovement != null && playerMovement.lastDirection.sqrMagnitude > 0.0001f)
                    ? playerMovement.lastDirection
                    : Vector2.down;

        Vector3Int offset;
        if (Mathf.Abs(d.x) >= Mathf.Abs(d.y))
            offset = (d.x >= 0) ? Vector3Int.right : Vector3Int.left;
        else
            offset = (d.y >= 0) ? Vector3Int.up : Vector3Int.down;

        Vector3Int targetCell = playerCell + offset;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector3Int clickedCell = soilTilemapController.groundTilemap.WorldToCell(mouseWorld);

        int range = 1;
        int dx = clickedCell.x - targetCell.x;
        int dy = clickedCell.y - targetCell.y;
        bool inRect = Mathf.Abs(dx) <= range && Mathf.Abs(dy) <= range;

        if (!inRect)
        {
            Debug.Log("[Hoe] 허용 범위 밖 클릭");
            return;
        }

        Vector3 center = soilTilemapController.groundTilemap.GetCellCenterWorld(clickedCell);
        bool ok = soilTilemapController.TryTillAtWorldPos(center);

        if (ok) StartToolAction(ToolType.Hoe);
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
        Vector3 world = GetTargetWorldPos();
        if (soilTilemapController.TryWaterAtWorldPos(world))
        {
            Debug.Log("[Water] 물 주기 성공");
            StartToolAction(ToolType.WateringCan);
            // (선택) 물통 용량 줄이려면 여기서 감소
        }
        else
        {
            Debug.Log("[Water] 물 주기 실패 (갈리지 않았거나 이미 물 먹음)");
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
            StartToolAction(ToolType.Sword);
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
                StartToolAction(tool.toolType);
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

    void StartToolAction(ToolType toolType)
    {
        if (!animator) return;

        animator.SetFloat(ParamToolIndex, (float)toolType);
        animator.ResetTrigger(TrigStartTool);
        animator.SetTrigger(TrigStartTool);
    }

    void UpdateUseToolPoint()
    {
        if (useToolPoint == null || playerMovement == null) return;

        Vector2 d = (playerMovement.lastDirection.sqrMagnitude > 0.0001f)
      ? playerMovement.lastDirection
      : Vector2.down;

        Vector3 offset;
        if (Mathf.Abs(d.x) >= Mathf.Abs(d.y))
            offset = (d.x > 0) ? Vector3.right : Vector3.left;
        else
            offset = (d.y > 0) ? Vector3.up : Vector3.down;

        float distance = 0.6f;

        float flipFix = (transform.localScale.x < 0) ? -1f : 1f;
        useToolPoint.localPosition = new Vector3(offset.x * distance * flipFix, offset.y * distance, 0);
    }
    Vector3 GetTargetWorldPos()
    {
        if (useMouseTarget)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            return mouseWorld;
        }
        else
        {
            // 플레이어 앞칸의 중심(현재 useToolPoint 위치 사용)
            return useToolPoint ? useToolPoint.position : transform.position;
        }
    }

    void TryPlantSeed(Seeds seed)
    {
        if (!soilTilemapController)
        {
            Debug.LogWarning("[Seed] SoilTilemapController가 없음");
            return;
        }

        Vector3 world = GetTargetWorldPos();

        // Soil 쪽에 TryPlantAtWorldPos 구현해둔 버전 (추천)
        if (soilTilemapController.TryPlantAtWorldPos(world, seed))
        {
            Debug.Log("[Seed] 심기 성공");
            // 인벤토리에서 씨앗 1개 차감 (너의 Inventory 규칙에 맞게)
            inventory.RemoveItem(seed, 1);
            StartToolAction(ToolType.Hoe); // 심을 때도 짧은 모션 쓰고 싶으면
        }
        else
        {
            Debug.Log("[Seed] 심기 실패 (갈리지 않았거나 이미 작물 있음)");
        }
    }

    void TryHarvestAtTarget()
    {
        if (!soilTilemapController)
        {
            Debug.LogWarning("[Harvest] SoilTilemapController가 없음");
            return;
        }

        Vector3 world = GetTargetWorldPos();
        if (soilTilemapController.TryHarvestAtWorldPos(world, out var harvested))
        {
            Debug.Log("[Harvest] 수확 성공");
            if (harvested != null)
                inventory?.AddItem(harvested, 1);
            // 낫 애니메이션을 쓰고 싶으면:
            StartToolAction(ToolType.Scythe);
        }
        else
        {
            Debug.Log("[Harvest] 수확할 성숙 작물이 없음");
        }
    }
}
