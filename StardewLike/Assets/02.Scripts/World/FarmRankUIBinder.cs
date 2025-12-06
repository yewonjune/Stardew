using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FarmRankUIBinder : MonoBehaviour
{
    [Header("Panel / Layout")]
    public GameObject coopPanel;          // FarmBoardPanel

    [Header("User Info Texts")]
    public Text idText;                   // IDText → PlayerRankManager.emailText 로 매핑
    public Text nameText;                 // NameText → nicknameText
    public Text farmNameText;             // FarmNameText
    public Text goldText;                 // GoldText
    public Text farmLevelText;            // FarmLevelText (N일차 표시용)

    [Header("Rank List")]
    public Transform goldRankContent;     // ScrollView/Viewport/Content
    public GameObject goldRankBlockPrefab;

    void Start()
    {
        if (PlayerRankManager.Instance != null)
        {
            PlayerRankManager.Instance.BindUI(
                coopPanel,
                idText,            // emailText
                nameText,          // nicknameText
                farmNameText,
                goldText,
                farmLevelText,     // seasonText
                goldRankContent,
                goldRankBlockPrefab
            );
        }
        else
        {
            Debug.LogWarning("[FarmRankUIBinder] PlayerRankManager.Instance 없음");
        }

        // 처음에는 게시판 패널 숨겨두기
        if (coopPanel != null)
            coopPanel.SetActive(false);
    }
}
