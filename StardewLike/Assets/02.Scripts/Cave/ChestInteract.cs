using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestInteract : MonoBehaviour
{
    public KeyCode openKey = KeyCode.E;
    public float interactDistance = 1.5f;

    Animator anim;
    bool opened = false;
    bool rewardGiven = false;
    Transform player;

    public Item rewardItem;
    public int rewardCount = 1;

    // 보상 아이템

    void Awake()
    {
        anim = GetComponent<Animator>();

        var go = GameObject.FindGameObjectWithTag("Player");
        if (go) player = go.transform;
    }

    void Update()
    {
        if (opened) return;
        if (!player) return;

        if (Input.GetKeyDown(openKey) && InRange())
        {
            Open();
        }
    }

    void Open()
    {
        opened = true;

        if (anim != null)
            anim.SetTrigger("Open");
        else
            GiveReward();
    }

    bool InRange()
    {
        float sqr = ((Vector2)player.position - (Vector2)transform.position).sqrMagnitude;
        return sqr <= interactDistance * interactDistance;
    }

    public void GiveReward()
    {
        if (rewardGiven) return;
        rewardGiven = true;

        if (rewardItem == null || rewardCount <= 0) return;

        if (Inventory.instance == null)
        {
            Debug.LogWarning("[ChestInteract] Inventory.instance가 없습니다!");
            return;
        }

        bool ok = Inventory.instance.AddItem(rewardItem, rewardCount);

        if (!ok)
        {
            Debug.Log("[ChestInteract] 인벤이 가득 차서 보상 지급 실패! (상자 다시 열 수 있게 되돌림)");
            opened = false;
            rewardGiven = false;
        }
    }

    }
