using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestBoardInteractor : MonoBehaviour
{
    public string playerTag = "Player";

    public KeyCode interactKey = KeyCode.E;
    [SerializeField] QuestBoardUI boardUI;
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
        if (boardUI.IsOpen) return;

        float dist = Vector2.Distance(player.position, transform.position);

        if (dist <= interactDistance && Input.GetKeyDown(interactKey))
        {
            boardUI.Open();
        }
    }
}
