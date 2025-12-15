using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestListItemUI : MonoBehaviour
{
    [SerializeField] Text titleText;
    [SerializeField] Text descText;
    [SerializeField] Text rewardText;
    [SerializeField] Text progressText;
    [SerializeField] Button button;

    public void Bind(QuestData data, QuestState state, System.Action onClick)
    {
        if (titleText) titleText.text = data != null ? data.title : state.questId;

        if (descText)
        {
            string d = data != null ? data.description : "";
            descText.text = Shorten(d, 30);
        }

        int gold = (data != null && data.reward != null) ? data.reward.gold : 0;
        if (rewardText) rewardText.text = $"보상 : {gold}G";

        if (progressText)
        {
            if (state.completed)
            {
                progressText.text = "완료";
            }
            else if (data != null)
            {
                var (cur, req) = GetTotalProgress(data, state);
                progressText.text = $"{cur}/{req}";
            }
            else
            {
                progressText.text = "";
            }
        }

        if (button)
        {
            button.onClick.RemoveAllListeners();

            if (state.completed)
            {
                button.interactable = false;
                SetAlpha(0.45f);
            }
            else
            {
                button.interactable = true;
                SetAlpha(1f);
                if (onClick != null) button.onClick.AddListener(() => onClick());
            }
        }
    }
    (int cur, int req) GetTotalProgress(QuestData data, QuestState state)
    {
        int cur = 0, req = 0;
        for (int i = 0; i < data.objectives.Count; i++)
        {
            req += data.objectives[i].requiredCount;
            int c = (state.currentCounts != null && i < state.currentCounts.Count) ? state.currentCounts[i] : 0;
            cur += Mathf.Min(c, data.objectives[i].requiredCount);
        }
        return (cur, req);
    }

    string Shorten(string s, int max)
    {
        if (string.IsNullOrEmpty(s)) return "";
        s = s.Replace("\n", " ");
        return s.Length <= max ? s : s.Substring(0, max) + "...";
    }

    void SetAlpha(float a)
    {
        if (titleText) titleText.color = new Color(titleText.color.r, titleText.color.g, titleText.color.b, a);
        if (descText) descText.color = new Color(descText.color.r, descText.color.g, descText.color.b, a);
        if (rewardText) rewardText.color = new Color(rewardText.color.r, rewardText.color.g, rewardText.color.b, a);
        if (progressText) progressText.color = new Color(progressText.color.r, progressText.color.g, progressText.color.b, a);
    }

}
