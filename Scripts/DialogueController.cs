using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text nameText;
    public Image portraitImage;

    [Header("Choice System")]
    public Transform choicesContainer;
    public GameObject choiceButtonPrefab;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// แสดง/ซ่อน Dialogue UI
    /// </summary>
    public void ShowDialogueUI(bool show)
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(show);
        }
    }

    /// <summary>
    /// ตั้งค่าข้อมูล NPC (ชื่อและรูป)
    /// </summary>
    public void SetNPCInfo(string npcName, Sprite portrait)
    {
        if (nameText != null)
        {
            nameText.text = npcName;
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = portrait;
        }
    }

    /// <summary>
    /// ตั้งค่าข้อความบทสนทนา
    /// </summary>
    public void SetDialogueText(string text)
    {
        if (dialogueText != null)
        {
            dialogueText.text = text;
        }
    }

    /// <summary>
    /// ลบตัวเลือกทั้งหมด
    /// </summary>
    public void ClearChoices()
    {
        if (choicesContainer != null)
        {
            foreach (Transform child in choicesContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }

    /// <summary>
    /// เพิ่มตัวเลือกใหม่ (ชื่อเดิม)
    /// </summary>
    public void AddChoice(string choiceText, System.Action onChoiceSelected)
    {
        if (choiceButtonPrefab == null || choicesContainer == null)
        {
            Debug.LogError("⚠️ ChoiceButtonPrefab หรือ ChoicesContainer เป็น null!");
            return;
        }

        GameObject choiceButton = Instantiate(choiceButtonPrefab, choicesContainer);

        TMP_Text buttonText = choiceButton.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.text = choiceText;
        }

        Button button = choiceButton.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() =>
            {
                onChoiceSelected?.Invoke();
                ClearChoices();
            });
        }
    }

    /// <summary>
    /// สร้างปุ่มตัวเลือก (ชื่อใหม่ - ให้รองรับ NPC.cs)
    /// </summary>
    public void CreateChoiceButton(string choiceText, System.Action onChoiceSelected)
    {
        // เรียก AddChoice เหมือนเดิม
        AddChoice(choiceText, onChoiceSelected);
    }
}