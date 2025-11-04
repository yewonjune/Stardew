using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCAffection : MonoBehaviour
{
    public string npcId;
    public int maxAffection = 100;
    public int currentAffection = 0;


    public void AddAffection(int amount)
    {
        currentAffection += amount;
        currentAffection = Mathf.Clamp(currentAffection, 0, maxAffection);
    }
}
