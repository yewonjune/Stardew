using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmBoardInteract : MonoBehaviour
{
    public string playerTag = "Player";

    public KeyCode interactKey = KeyCode.E;

    public float interactDistance = 1.2f;
    Transform player;

    void Start()
    {
        var go = GameObject.FindGameObjectWithTag(playerTag);
        if (go) player = go.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (!player) return;

        float dist = Vector2.Distance(player.position, transform.position);

        if (dist <= interactDistance && Input.GetKeyDown(interactKey))
        {
            ToggleBoard();
        }
    }
    void ToggleBoard()
    {
        var rank = PlayerRankManager.Instance;
        if (rank == null)  return;


        if (rank.farmboardPanel != null && rank.farmboardPanel.activeSelf)
        {
            // 이미 열려 있으면 닫기
            rank.CloseRankPanel();
        }
        else
        {
            // 닫혀 있으면 열기
            rank.OpenCoopPanelFromBoard();
        }
    }
}
