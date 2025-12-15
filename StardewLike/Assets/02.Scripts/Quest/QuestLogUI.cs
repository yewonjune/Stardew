using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class QuestLogUI : MonoBehaviour
{
    [SerializeField] GameObject panel;

    [SerializeField] Transform listContent;
    [SerializeField] GameObject itemPrefab;

    string selectedQuestId;

    void Awake()
    {
        if(panel) panel.SetActive(false);
    }

    void OnEnable()
    {
        if (QuestManager.I != null)
            QuestManager.I.OnQuestChanged += Rebuild;
    }

    void OnDisable()
    {
        if (QuestManager.I != null)
            QuestManager.I.OnQuestChanged -= Rebuild;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            Toggle();

        if (panel != null && panel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            Toggle();
    }

    public void Toggle()
    {
        if (DialogueManager.IsBusy) return;

        bool open = !panel.activeSelf;
        panel.SetActive(open);

        if (open)
        {
            PlayerActionLock.Lock("QuestLog");
            Rebuild();
        }
        else
        {
            PlayerActionLock.Unlock("QuestLog");
        }
    }

    void Rebuild()
    {
        var qm = QuestManager.I;
        if (qm == null) return;

        // 기존 항목 제거
        for (int i = listContent.childCount - 1; i >= 0; i--)
            Destroy(listContent.GetChild(i).gameObject);

        if (qm.Active.Count == 0)
        {
            return;
        }

        var ordered = qm.Active.Values
            .OrderBy(s => s.completed)
            .ThenBy(s => s.questId);

        foreach (var state in ordered)
        {
            var data = qm.GetQuestData(state.questId);

            var go = Instantiate(itemPrefab, listContent);
            var itemUI = go.GetComponent<QuestListItemUI>();

            if (itemUI == null)
            {
                Debug.LogError("[QuestLogUI] itemPrefab에 QuestListItemUI 컴포넌트가 없습니다.");
                continue;
            }

            itemUI.Bind(data, state, onClick: null);
        }
    }

}
