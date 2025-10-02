using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    Inventory inventory;

    public GameObject InventoryPanel;
    bool activeInventory = false;

    public Slot[] slots;
    public Transform slotHolder;


    void Start()
    {
        inventory = Inventory.instance;
        slots = slotHolder.GetComponentsInChildren<Slot>();
        inventory.onSlotCountChange += SlotChange;
        InventoryPanel.SetActive(activeInventory);
    }

    void SlotChange(int val)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inventory.SlotCnt)
            {
                slots[i].GetComponent<Button>().interactable = true;
            }
            else
            {
                slots[i].GetComponent<Button>().interactable = false;
            }
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            activeInventory = !activeInventory;
            InventoryPanel.SetActive(activeInventory);
        }
        
    }

    public void AddSlot()
    {
        inventory.SlotCnt += 10;
    }

}
