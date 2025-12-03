using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CaveFloorUI : MonoBehaviour
{
    [SerializeField] Text floorText;

    // Start is called before the first frame update
    void Start()
    {
        UpdateFloorUI();
    }

    public void UpdateFloorUI()
    {
        int floor = CaveStateManager.CurrentFloor;

        if (floorText != null)
        {
            floorText.text = $"{floor}";
        }    
    }
}
