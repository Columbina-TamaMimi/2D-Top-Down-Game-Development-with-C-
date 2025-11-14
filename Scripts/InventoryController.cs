using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    private ItemDictionary itemDictionary;
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefabs;

    public static InventoryController Instance { get; private set; }
    Dictionary<int, int> itemsCountCache = new();
    public event Action OnInventoryChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        itemDictionary = FindObjectOfType<ItemDictionary>();
        RebuildItemCounts();
    }

    public void RebuildItemCounts()
    {
        itemsCountCache.Clear();

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null)
                {
                    itemsCountCache[item.ID] = itemsCountCache.GetValueOrDefault(item.ID, 0) + item.quantity;
                }
            }
        }

        OnInventoryChanged?.Invoke();
    }

    public Dictionary<int, int> GetItemCounts() => itemsCountCache;

    /// <summary>
    /// ✅ ลบไอเทมจาก Inventory
    /// </summary>
    public bool RemoveItem(int itemID, int amountToRemove)
    {
        if (amountToRemove <= 0)
        {
            Debug.LogWarning("⚠️ จำนวนที่จะลบต้องมากกว่า 0");
            return false;
        }

        int remainingToRemove = amountToRemove;
        List<Slot> slotsToUpdate = new List<Slot>();

        // หา Slot ทั้งหมดที่มีไอเทมนี้
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            if (remainingToRemove <= 0) break;

            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null && item.ID == itemID)
                {
                    slotsToUpdate.Add(slot);
                }
            }
        }

        // ลบไอเทมจากแต่ละ Slot
        foreach (Slot slot in slotsToUpdate)
        {
            if (remainingToRemove <= 0) break;

            Item item = slot.currentItem.GetComponent<Item>();
            if (item != null)
            {
                int removeAmount = Mathf.Min(remainingToRemove, item.quantity);
                item.quantity -= removeAmount;
                remainingToRemove -= removeAmount;

                Debug.Log($"🗑️ ลบไอเทม ID:{itemID} จาก Slot จำนวน {removeAmount} (เหลือ {item.quantity})");

                // ถ้าจำนวนเป็น 0 → ลบไอเทมออกจาก Slot
                if (item.quantity <= 0)
                {
                    Destroy(slot.currentItem);
                    slot.currentItem = null;
                    Debug.Log($"🗑️ Slot ว่าง");
                }
                else
                {
                    // อัพเดทจำนวนที่แสดง (ถ้ามี UI แสดงจำนวน)
                    item.UpdateQuantityDisplay();
                }
            }
        }

        // อัพเดท Inventory
        RebuildItemCounts();

        // เช็คว่าลบหมดหรือยัง
        if (remainingToRemove > 0)
        {
            Debug.LogWarning($"⚠️ ไอเทม ID:{itemID} ไม่พอลบ (ขาดอีก {remainingToRemove})");
            return false;
        }

        Debug.Log($"✅ ลบไอเทม ID:{itemID} จำนวน {amountToRemove} สำเร็จ");
        return true;
    }

    /// <summary>
    /// ✅ เช็คว่ามีไอเทมพอหรือไม่
    /// </summary>
    public bool HasItem(int itemID, int requiredAmount)
    {
        int currentAmount = itemsCountCache.GetValueOrDefault(itemID, 0);
        return currentAmount >= requiredAmount;
    }

    /// <summary>
    /// ✅ ดึงจำนวนไอเทมที่มี
    /// </summary>
    public int GetItemCount(int itemID)
    {
        return itemsCountCache.GetValueOrDefault(itemID, 0);
    }

    public void QuitGame()
    {
        Debug.Log("ออกจากเกม");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public List<InventorySaveData> GetInventoryItem()
    {
        List<InventorySaveData> invData = new List<InventorySaveData>();
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null)
                {
                    invData.Add(new InventorySaveData
                    {
                        itemID = item.ID,
                        slotIndex = slotTransform.GetSiblingIndex()
                    });
                }
            }
        }
        return invData;
    }

    public void SetInventoryItems(List<InventorySaveData> inventorySaveData)
    {
        if (itemDictionary == null)
        {
            Debug.LogError("itemDictionary เป็น null! ไม่สามารถโหลด inventory ได้");
            itemDictionary = FindObjectOfType<ItemDictionary>();
            if (itemDictionary == null)
            {
                return;
            }
        }

        if (inventoryPanel == null)
        {
            Debug.LogError("inventoryPanel เป็น null!");
            return;
        }

        // Clear inventory panel
        foreach (Transform child in inventoryPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // Create new slots
        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, inventoryPanel.transform);
        }

        // Populate slots with saved items
        foreach (InventorySaveData data in inventorySaveData)
        {
            if (data.slotIndex < slotCount)
            {
                Transform slotTransform = inventoryPanel.transform.GetChild(data.slotIndex);
                if (slotTransform != null)
                {
                    Slot slot = slotTransform.GetComponent<Slot>();
                    if (slot != null)
                    {
                        GameObject itemPrefab = itemDictionary.GetItemByID(data.itemID);
                        if (itemPrefab != null)
                        {
                            GameObject item = Instantiate(itemPrefab, slot.transform);
                            item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                            slot.currentItem = item;
                        }
                        else
                        {
                            Debug.LogWarning($"ไม่พบ item prefab สำหรับ ID: {data.itemID}");
                        }
                    }
                }
            }
        }

        RebuildItemCounts();
    }

    public bool AddItem(int itemID)
    {
        if (itemDictionary == null)
        {
            Debug.LogError("itemDictionary เป็น null!");
            return false;
        }

        GameObject itemPrefab = itemDictionary.GetItemByID(itemID);
        if (itemPrefab == null)
        {
            Debug.LogWarning($"ไม่พบ item prefab สำหรับ ID: {itemID}");
            return false;
        }

        // หาช่องว่างใน inventory
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slotTransform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.currentItem = newItem;

                RebuildItemCounts();
                return true;
            }
        }

        Debug.Log("Inventory is full!");
        return false;
    }

    /// <summary>
    /// ✅ Debug: แสดงไอเทมทั้งหมด
    /// </summary>
    [ContextMenu("Show All Items")]
    void ShowAllItems()
    {
        Debug.Log("📦 Inventory Contents:");
        foreach (var kvp in itemsCountCache)
        {
            Debug.Log($"  - Item ID:{kvp.Key} x{kvp.Value}");
        }
    }
}