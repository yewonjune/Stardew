using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmBoardInteract : MonoBehaviour
{
    public string playerTag = "Player";

    public KeyCode interactKey = KeyCode.E;

    bool playerInRange = false;

    // Update is called once per frame
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            ToggleBoard();
        }

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            Debug.Log("[FarmBoardInteract] 플레이어 범위 안으로 들어옴");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            Debug.Log("[FarmBoardInteract] 플레이어 범위 밖으로 나감");
        }
    }
    void ToggleBoard()
    {
        var rank = PlayerRankManager.Instance;
        if (rank == null)
        {
            Debug.LogWarning("[FarmBoardInteract] PlayerRankManager.Instance가 없음!");
            return;
        }

        // coopPanel 상태 보고 열기/닫기 토글
        if (rank.coopPanel != null && rank.coopPanel.activeSelf)
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
