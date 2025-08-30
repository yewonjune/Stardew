using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUseTool : MonoBehaviour
{
    public HotbarManager hotbarManager;
    public Transform useToolPoint;

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
            case ToolType.Pickaxe:
                // 돌 깨기
                BreakRockWithPickaxe();
                break;
            case ToolType.Axe:
                // 나무 베기
                ChopTreeWithAxe();
                break;
            case ToolType.WateringCan:
                // 물 뿌리기
                WaterCropWithWateringCan();
                break;
            case ToolType.Scythe:
                // 작물 베기(?)
                HarvestCropWithScythe();
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
        RaycastHit2D hit = Physics2D.Raycast(useToolPoint.position, Vector2.zero);

        if(hit.collider != null)
        {
            Debug.Log("땅 파기");
        }
    }

    void BreakRockWithPickaxe()
    {
        RaycastHit2D hit = Physics2D.Raycast(useToolPoint.position, Vector2.zero);

        if (hit.collider != null)
        {
            Debug.Log("돌깨기");
        }
    }

    void ChopTreeWithAxe()
    {
        RaycastHit2D hit = Physics2D.Raycast(useToolPoint.position, Vector2.zero);

        if (hit.collider != null)
        {
            Debug.Log("나무베기");
        }
    }

    void WaterCropWithWateringCan()
    {
        RaycastHit2D hit = Physics2D.Raycast(useToolPoint.position, Vector2.zero);

        if (hit.collider != null)
        {
            Debug.Log("물뿌리기");
        }
    }

    void HarvestCropWithScythe()
    {
        RaycastHit2D hit = Physics2D.Raycast(useToolPoint.position, Vector2.zero);

        if (hit.collider != null)
        {
            Debug.Log("작물 베기");
        }
    }
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
}
