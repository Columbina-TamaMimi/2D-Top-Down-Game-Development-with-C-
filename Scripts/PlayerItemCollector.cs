using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItemCollector : MonoBehaviour
{
    private InventoryController inventoryController;

    void Awake()
    {
        inventoryController = FindObjectOfType<InventoryController>();

        if (inventoryController == null)
        {
            Debug.LogError("ไม่พบ InventoryController ใน Scene!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Item"))
        {
            Item item = collision.GetComponent<Item>();
            if (item != null && inventoryController != null)
            {
                // ส่ง item.ID (int) ไม่ใช่ item (GameObject)
                bool itemAdded = inventoryController.AddItem(item.ID);

                if (itemAdded)
                {
                    item.Pickup();
                    Debug.Log($"เก็บ {collision.gameObject.name} แล้ว!");
                    Destroy(collision.gameObject);
                }
                else
                {
                    Debug.Log("Inventory เต็ม! ไม่สามารถเก็บไอเทมได้");
                }
            }
        }
    }
}