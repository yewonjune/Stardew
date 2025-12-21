using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestBoardUI : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Text questDescText;
    [SerializeField] Button acceptBtn;

    [SerializeField] QuestData quest;

    bool isOpen;
    public bool IsOpen => isOpen;

    void Awake()
    {
        if (panel) panel.SetActive(false);

        if (acceptBtn)
        {
            acceptBtn.onClick.RemoveAllListeners();
            acceptBtn.onClick.AddListener(Accept);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isOpen) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    public void Open()
    {
        if (quest == null) return;

        if (!isOpen) //처음 열 때만 Lock
        {
            isOpen = true;
            panel.SetActive(true);
            PlayerActionLock.Lock("QuestBoard");
        }
        quest = QuestManager.I.GetRandomQuest();

        RefreshUI();
    }

    void RefreshUI()
    {
        if (quest == null)
        {
            if (questDescText) questDescText.text = "현재 수락 가능한 퀘스트가 없어.";
            if (acceptBtn) acceptBtn.interactable = false;
            return;
        }

        if (questDescText) questDescText.text = quest.description;

        bool alreadyAccepted = QuestManager.I.Active.ContainsKey(quest.questId);

        if (acceptBtn)
        {
            acceptBtn.interactable = !alreadyAccepted;

            var btnText = acceptBtn.GetComponentInChildren<Text>();
            if (btnText != null)
                btnText.text = alreadyAccepted ? "수락됨" : "수락";
        }
    }

    void Close()
    {
        isOpen = false;
        panel.SetActive(false);

        PlayerActionLock.Unlock("QuestBoard");
    }

    void Accept()
    {
        if (quest == null || QuestManager.I == null) return;

        bool ok = QuestManager.I.Accept(quest.questId);
        if (!ok) return;

        RefreshUI();
    }
}
