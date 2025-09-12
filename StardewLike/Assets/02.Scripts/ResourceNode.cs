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

    public GameObject dropPrefab;
    public int dropCount;

    private void Awake()
    {
        hp = maxHp;
    }

    public void Hit(Tools tool)
    {
        if(tool.toolType != requiredTool)
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
            Break();
    }

    void Break()
    {
        Debug.Log($"[Resource] {resourceType} 파괴됨!");

        //if (dropPrefab != null)
        //{
        //    for (int i = 0; i < dropCount; i++)
        //    {
        //        Instantiate(dropPrefab, transform.position, Quaternion.identity);
        //    }
        //}

        Destroy(gameObject);
    }

}
