using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] PlayerHealthController playerHealthController;
    [SerializeField] Image fill;

    private void Awake()
    {
        if(playerHealthController == null) 
            playerHealthController = FindObjectOfType<PlayerHealthController>();
    }

    void OnEnable()
    {
        if (playerHealthController)
            playerHealthController.OnHpChanged += Refresh;

        if (playerHealthController)
            Refresh(playerHealthController.currentHp, playerHealthController.maxHP);
    }

    void OnDisable()
    {
        if (playerHealthController)
            playerHealthController.OnHpChanged -= Refresh;
    }

    void Refresh(int cur, int max)
    {
        if (!fill) return;
        if (max <= 0) { fill.fillAmount = 0f; return; }

        fill.fillAmount = (float)cur / max; // HP는 남은만큼 채워짐
    }
}
