using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

public class PlayerUseTool : MonoBehaviour
{
    // ================== 레퍼런스 ==================
    [TitleGroup("References")]
    [FoldoutGroup("References/General", Expanded = true)]
    [LabelText("Hotbar Manager")]
    public HotbarManager hotbarManager;

    [FoldoutGroup("References/General")]
    [LabelText("Tool Pivot (앞칸 기준점)")]
    public Transform useToolPoint;

    [FoldoutGroup("References/General")]
    [LabelText("Animator")]
    public Animator animator;

    [FoldoutGroup("References/General")]
    [LabelText("Player Movement")]
    [SerializeField] PlayerMovement playerMovement;

    [FoldoutGroup("References/General")]
    [LabelText("Fatigue Controller")]
    [SerializeField] PlayerFatigueController playerFatigueController;

    [FoldoutGroup("References/General")]
    [LabelText("Soil Tilemap Controller")]
    [SerializeField] SoilTilemapController soilTilemapController;

    // ================== 공통 도구 설정 ==================
    [TitleGroup("Tool Settings")]
    [FoldoutGroup("Tool Settings/General", Expanded = true)]
    [LabelText("마우스로 지점 지정")]
    [SerializeField] bool useMouseTarget = false;

    [FoldoutGroup("Tool Settings/General")]
    [LabelText("자원 타격 반경")]
    [MinValue(0f)]
    [SerializeField] float hitRadius = 0.15f;

    [FoldoutGroup("Tool Settings/General")]
    [LabelText("자원 레이어 (돌/나무 등)")]
    [SerializeField] LayerMask resourceLayer = ~0;

    // ================== 검 설정 ==================
    [TitleGroup("Sword Settings")]
    [FoldoutGroup("Sword Settings/Hit", Expanded = true)]
    [LabelText("검 공격 반경")]
    [MinValue(0f)]
    [SerializeField] float swordAttackRadius = 0.6f;

    [FoldoutGroup("Sword Settings/Hit")]
    [LabelText("검 데미지")]
    [MinValue(0)]
    [SerializeField] int swordDamage = 10;

    [FoldoutGroup("Sword Settings/Hit")]
    [LabelText("몬스터 레이어")]
    [SerializeField] LayerMask enemyLayer;

    [FoldoutGroup("Sword Settings/Combo", Expanded = true)]
    [LabelText("콤보 허용 간격(초)")]
    [MinValue(0f)]
    [SerializeField] float swordComboMaxGap = 0.4f;

    [FoldoutGroup("Sword Settings/Combo")]
    [ShowInInspector, ReadOnly]
    [LabelText("현재 콤보 단계 (1~3)")]
    int swordCombo = 0;

    [FoldoutGroup("Sword Settings/Combo")]
    [ShowInInspector, ReadOnly]
    [LabelText("마지막 검 사용 시간")]
    float lastSwordClickTime = -999f;

    // ================== 애니메이터 파라미터 (고정값) ==================
    const string ParamToolIndex = "ToolIndex";
    const string TrigStartTool = "StartAction_Tool";
    const string TrigSwordAttack = "SwordAttack";
    const string ParamSwordCombo = "SwordCombo";

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        RebindSoilController();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    void OnActiveSceneChanged(Scene prev, Scene next)
    {
        RebindSoilController();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene == SceneManager.GetActiveScene())
            RebindSoilController();
    }

    void RebindSoilController()
    {
        soilTilemapController = null;

        var all = FindObjectsOfType<SoilTilemapController>(includeInactive: false);
        if (all == null || all.Length == 0)
        {
            return;
        }

        float best = float.PositiveInfinity;
        SoilTilemapController bestCtrl = null;
        Vector3 p = transform.position;

        foreach (var s in all)
        {
            if (!s.isActiveAndEnabled) continue;
            float d = (s.transform.position - p).sqrMagnitude;
            if (d < best) { best = d; bestCtrl = s; }
        }

        soilTilemapController = bestCtrl ?? all[0];
    }

    // Update is called once per frame
    void Update()
    {
        if (GamePause.isPaused || DialogueManager.IsBusy) return;

        UpdateUseToolPoint();

        if (Input.GetMouseButtonDown(0))
        {
            if (IsClickOnBlockingUI())
            { 
                return;
            }

            var fc = GetComponent<PlayerFishingController>();
            if (fc != null && fc.isFishing) return;

            Item selectedItem = hotbarManager.GetSelectedItem();

            if (selectedItem is Tools tool)
            {
                UseTool(tool);
            }
            else if (selectedItem is Seeds seed)
            {
                TryPlantSeed(seed);
            }
            else
            {
                TryHarvestAtTarget();
            }
        }
    }

    void UseTool(Tools tool)
    {
        if (playerFatigueController && playerFatigueController.IsFull)
        {
            Debug.Log("오늘 하루 체력을 다 사용했습니다");
            return;
        }

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
        if (!soilTilemapController) return;

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

        if (ok)
        {
            StartToolAction(ToolType.Hoe);
            playerFatigueController?.AddByTool(ToolType.Hoe);
        }
    }

    void WaterCropWithWateringCan()
    {
        if (!soilTilemapController) return;

        Vector3 world = GetTargetWorldPos();
        if (soilTilemapController.TryWaterAtWorldPos(world))
        {
            StartToolAction(ToolType.WateringCan);
            playerFatigueController?.AddByTool(ToolType.WateringCan);
            // 물통 용량 줄이려면 여기서 감소
        }
        else
        {
            Debug.Log("[Water] 물 주기 실패 (갈리지 않았거나 이미 물 먹음)");
        }
    }

    void AttackWithSword()
    {
        float now = Time.time;
        if (now - lastSwordClickTime > swordComboMaxGap)
        {
            swordCombo = 1;
        }
        else
        {
            swordCombo = Mathf.Clamp(swordCombo + 1, 1, 3);
        }
        lastSwordClickTime = now;

        if (animator)
        {
            animator.SetInteger(ParamSwordCombo, swordCombo);
            animator.ResetTrigger(TrigSwordAttack);
            animator.SetTrigger(TrigSwordAttack);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            useToolPoint.position,
            swordAttackRadius,
            enemyLayer
        );

        foreach (var col in hits)
        {
            var enemy = col.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                Vector2 hitDir = (enemy.transform.position - transform.position).normalized;
                enemy.TakeDamage(swordDamage, hitDir);
            }
        }

        // playerFatigueController?.AddByTool(ToolType.Sword);
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
        {
            fishingController.TryStopFishing();
        }
        else
        {
            fishingController.TryStartFishing();
        }
    }

    void BreakResourceWithTool(Tools tool)
    {
        Collider2D collider = Physics2D.OverlapCircle(useToolPoint.position, hitRadius, resourceLayer);
        if (collider != null)
        {
            ResourceNode resourceNode = collider.GetComponent<ResourceNode>();
            CaveResourceNode caveNode = collider.GetComponent<CaveResourceNode>();

            if (resourceNode != null)
            {
                resourceNode.Hit(tool);
                StartToolAction(tool.toolType);
                playerFatigueController?.AddByTool(tool.toolType);
                return;
            }

            if (caveNode != null)
            {
                caveNode.Hit(tool);
                StartToolAction(tool.toolType);
                playerFatigueController?.AddByTool(tool.toolType);
                return;
            }
        }

        if (tool.toolType == ToolType.Pickaxe && soilTilemapController != null)
        {
            Vector3 world = GetTargetWorldPos();

            if (soilTilemapController.TryClearSoilAtWorldPos(world))
            {
                StartToolAction(tool.toolType);
                playerFatigueController?.AddByTool(tool.toolType);
            }
        }
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
            return;
        }

        if (SeasonManager.Instance != null && !seed.CanGrowInSeason(SeasonManager.Instance.currentSeason))
        {
            Debug.Log("이 계절에는 이 씨앗을 심을 수 없어요!");
            return;
        }

        Vector3 world = GetTargetWorldPos();

        if (soilTilemapController.TryPlantAtWorldPos(world, seed))
        {
            Inventory.instance.RemoveItem(seed, 1);
            //StartToolAction(ToolType.Hoe); // 모션 나중에 변경
        }
    }

    void TryHarvestAtTarget()
    {
        if (!soilTilemapController)
        {
            return;
        }

        Vector3 world = GetTargetWorldPos();
        if (soilTilemapController.TryHarvestAtWorldPos(world, out var harvested))
        {
            if (harvested != null)
                Inventory.instance?.AddItem(harvested, 1);
            
            //StartToolAction(ToolType.Scythe);   // 나중에 추가
        }
    }

    bool IsClickOnBlockingUI()
    {
        if (EventSystem.current == null) return false;

        PointerEventData data = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        for (int i = 0; i < results.Count; i++)
        {
            if (results[i].gameObject.CompareTag("BlockTool"))
                return true;
        }

        return false;
    }
}
