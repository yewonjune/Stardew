using UnityEngine;

public class EscMenuController : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        if (settingsPanel.activeSelf)
        {
            // 설정창 열려있으면 ESC로 닫기
            CloseSettings();
            return;
        }

        // 다른 UI가 ESC를 이미 처리 중이면 설정창 열지 않음
        // (DialogueManager, QuestBoardUI, QuestLogUI가 각자 처리)
        if (DialogueManager.IsBusy) return;
        if (PlayerActionLock.IsLocked) return; // 아래 참고

        OpenSettings();
    }

    void OpenSettings()
    {
        settingsPanel.SetActive(true);
        PlayerActionLock.Lock("Settings");
        Time.timeScale = 0f; // 설정창 열리면 게임 일시정지
    }

    void CloseSettings()
    {
        settingsPanel.SetActive(false);
        PlayerActionLock.Unlock("Settings");
        Time.timeScale = 1f;
    }
}