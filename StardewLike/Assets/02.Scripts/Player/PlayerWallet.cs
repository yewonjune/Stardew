using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    public static PlayerWallet Instance;
    [SerializeField] public int gold = 500;

    void Awake()
    {
        if (Instance != null)
        { 
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool CanPay(int cost) => gold >= cost;

    public bool TryPay(int cost)
    {
        if (!CanPay(cost)) return false;
        gold -= cost;
        return true;
    }

    public void Earn(int amount) => gold += amount;
}
