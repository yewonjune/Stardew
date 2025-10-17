using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item itemData;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            bool added = Inventory.instance.AddItem(itemData);
            if (added)
            {
                Destroy(gameObject);
            }
        }
    }
}
