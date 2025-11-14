using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    public bool IsOpened { get; private set; }
    public string ChestID { get; private set; }

    [Header("Item Settings")]
    public GameObject itemPrefab; // Item that chest drops

    [Header("Visual Settings")]
    public Sprite openedSprite;

    [Header("Sound Settings")]
    [SerializeField] private string chestOpenSoundName = "Chest"; // ชื่อ Sound Group สำหรับเปิดหีบ
    [SerializeField] private bool playSound = true; // เปิด/ปิดเสียง

    void Start()
    {
        ChestID ??= GlobalHelper.GenerateUniqueID(gameObject);
    }

    public bool CanInteract()
    {
        return !IsOpened;
    }

    public void Interact()
    {
        if (!CanInteract()) return;
        OpenChest();
    }

    private void OpenChest()
    {
        SetOpened(true);

        // 🔊 เล่นเสียงเปิดหีบ
        PlayOpenSound();

        // DropItem
        if (itemPrefab)
        {
            GameObject droppedItem = Instantiate(itemPrefab, transform.position + Vector3.down, Quaternion.identity);
            droppedItem.GetComponent<BounceEffect>().StartBounce();
        }
    }

    /// <summary>
    /// 🔊 เล่นเสียงเปิดหีบ
    /// </summary>
    private void PlayOpenSound()
    {
        if (!playSound) return;

        if (SoundEffectManager.Instance != null)
        {
            SoundEffectManager.Instance.Play(chestOpenSoundName);
            Debug.Log($"🔊 เล่นเสียงเปิดหีบ: {chestOpenSoundName}");
        }
        else
        {
            Debug.LogWarning("⚠️ ไม่พบ SoundEffectManager!");
        }
    }

    public void SetOpened(bool opened)
    {
        IsOpened = opened;

        if (IsOpened && openedSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = openedSprite;
        }
    }
}