using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Item : MonoBehaviour
{
    public int ID;
    public string Name;
    public int quantity = 1;

    public virtual void Pickup()
    {
        Sprite itemIcon = GetComponent<SpriteRenderer>()?.sprite;
        if (ItemPickupUIController.Instance != null && itemIcon != null)
        {
            ItemPickupUIController.Instance.ShowItemPickup(Name, itemIcon);
        }
    }

    /// <summary>
    /// ✅ อัพเดทการแสดงจำนวนไอเทม
    /// </summary>
    public void UpdateQuantityDisplay()
    {
        // หา Text Component ใน Children
        // ลองหา TextMeshProUGUI ก่อน (TMP)
        TextMeshProUGUI tmpText = GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null)
        {
            // ถ้ามีมากกว่า 1 ชิ้น → แสดงจำนวน
            // ถ้าเป็น 1 ชิ้น → ไม่แสดง (เว้นว่าง)
            tmpText.text = quantity > 1 ? quantity.ToString() : "";
            return;
        }

        // ถ้าไม่มี TMP ลองหา Text ธรรมดา
        Text legacyText = GetComponentInChildren<Text>();
        if (legacyText != null)
        {
            legacyText.text = quantity > 1 ? quantity.ToString() : "";
            return;
        }

        // ถ้าไม่มี Text ทั้ง 2 แบบ = ไม่ต้องแสดงจำนวน
        // (บางเกมอาจไม่มี UI แสดงจำนวน)
    }

    /// <summary>
    /// ✅ เพิ่มจำนวนไอเทม
    /// </summary>
    public void AddQuantity(int amount)
    {
        quantity += amount;
        UpdateQuantityDisplay();
        Debug.Log($"📦 เพิ่มไอเทม '{Name}' จำนวน {amount} (รวม {quantity})");
    }

    /// <summary>
    /// ✅ ลดจำนวนไอเทม
    /// </summary>
    public void RemoveQuantity(int amount)
    {
        quantity -= amount;

        if (quantity < 0)
            quantity = 0;

        UpdateQuantityDisplay();
        Debug.Log($"🗑️ ลบไอเทม '{Name}' จำนวน {amount} (เหลือ {quantity})");
    }

    /// <summary>
    /// ✅ ตั้งค่าจำนวนไอเทม
    /// </summary>
    public void SetQuantity(int newQuantity)
    {
        quantity = newQuantity;
        UpdateQuantityDisplay();
    }
}