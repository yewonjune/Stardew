using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerWallet : MonoBehaviour
{
    public static PlayerWallet Instance;

    public Text goldText;
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

    void Start()
    {
        RefreshUI();
    }

    public bool CanPay(int cost) => gold >= cost;

    public bool TryPay(int cost)
    {
        if (!CanPay(cost)) return false;
        gold -= cost;
        RefreshUI();

        return true;
    }

    public void Earn(int amount)
    {
        gold += amount;
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (goldText != null)
            goldText.text = $"{gold}";
    }
}
