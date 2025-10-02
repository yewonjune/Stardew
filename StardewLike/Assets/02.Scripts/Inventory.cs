using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    #region Singleton
    public static Inventory instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(instance);
        }

        instance = this;
    }
    #endregion

    public delegate void OnSlotCountChange(int val);
    public OnSlotCountChange onSlotCountChange;

    private int slotCnt;

    public int SlotCnt
    {
        get => slotCnt;
        set
        {
            slotCnt = value;
            onSlotCountChange.Invoke(slotCnt);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        SlotCnt = 10;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
