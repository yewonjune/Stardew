using UnityEngine;

public class FarmBoardInteract : MonoBehaviour, IInteractable
{
    public string playerTag = "Player";
    public float interactDistance = 1.2f;   // PlayerInteraction의 range로 통합되면 이 필드도 제거 가능
    Transform player;

    public string InteractLabel => "게시판 보기";

    public void Interact() => ToggleBoard();

    // Update() 전체 제거

    void Start()
    {
        var go = GameObject.FindGameObjectWithTag(playerTag);
        if (go) player = go.transform;
    }

    void ToggleBoard()
    {
        var rank = PlayerRankManager.Instance;
        if (rank == null) return;

        if (rank.farmboardPanel != null && rank.farmboardPanel.activeSelf)
            rank.CloseRankPanel();
        else
            rank.OpenCoopPanelFromBoard();
    }
}