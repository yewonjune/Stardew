using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthController : MonoBehaviour
{
    public int maxHP = 20;
    public int currentHp;

    public event Action<int, int> OnHpChanged;
    public event Action OnDead;

    bool deadFlowStarted;

    void Awake()
    {
        currentHp = maxHP;
        OnHpChanged?.Invoke(currentHp, maxHP);
    }

    public void TakeDamage(int damage)
    {
        if (deadFlowStarted) return;

        currentHp = Mathf.Max(0, currentHp - damage);
        OnHpChanged?.Invoke(currentHp, maxHP);

        if (currentHp <= 0)
        {
            deadFlowStarted = true;
            OnDead?.Invoke();
        }
    }

    public void SetHp(int value)
    {
        currentHp = Mathf.Clamp(value, 0, maxHP);
        deadFlowStarted = (currentHp <= 0);
        OnHpChanged?.Invoke(currentHp, maxHP);
    }

}
