using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FatigueBarUI : MonoBehaviour
{
    [SerializeField] PlayerFatigueController playerFatigueController;
    [SerializeField] Image fill;
    void OnEnable()
    {
        if (playerFatigueController)
            playerFatigueController.OnFatigueChanged += Refresh;
        if (playerFatigueController)
            Refresh(playerFatigueController.current, playerFatigueController.maxFatigue);
    }

    void OnDisable()
    {
        if (playerFatigueController)
            playerFatigueController.OnFatigueChanged -= Refresh;
    }

    void Refresh(float cur, float max)
    {
        if (!fill) return;
        if (max <= 0) { fill.fillAmount = 0f; return; }

        fill.fillAmount = 1f - (cur / max);
    }
}
