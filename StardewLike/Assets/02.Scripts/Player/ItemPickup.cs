using System.Collections;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item itemData;
    public float pickupDelay = 0.5f;
    private bool canPickup = false;
    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        StartCoroutine(EnablePickupAfterDelay());
    }

    IEnumerator EnablePickupAfterDelay()
    {
        if (col != null) col.enabled = false;  // 처음엔 꺼두기
        yield return new WaitForSeconds(pickupDelay);
        if (col != null) col.enabled = true;   // 나중에 켜기
        canPickup = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canPickup) return;

        if (collision.CompareTag("Player"))
        {
            bool added = Inventory.instance.AddItem(itemData);
            if (added)
            {
                Destroy(gameObject);
            }
            else
            {
                var drop = GetComponent<DropItemController>();
                if (drop != null) drop.OnCollectFailed();
            }
        }
    }
}
