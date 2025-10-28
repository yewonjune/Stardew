using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FatigueBarUI : MonoBehaviour
{
    [SerializeField] PlayerFatigueController playerFatigueControllercontroller;
    [SerializeField] Image fill;
    void OnEnable()
    {
        if (playerFatigueControllercontroller) 
            playerFatigueControllercontroller.OnFatigueChanged += Refresh;
        if (playerFatigueControllercontroller)
            Refresh(playerFatigueControllercontroller.current, playerFatigueControllercontroller.maxFatigue);
    }

    void OnDisable()
    {
        if (playerFatigueControllercontroller) 
            playerFatigueControllercontroller.OnFatigueChanged -= Refresh;
    }

    void Refresh(float cur, float max)
    {
        if (!fill) return;
        fill.fillAmount = max > 0 ? cur / max : 0f;
    }
}
