using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour, IInteractable
{
    public NPCDialogue dialogueData;
    private DialogueController dialogueUI;
    private int dialogueIndex;
    private bool isTyping, isDialogueActive;

    private enum QuestState { NotStarted, InProgress, Completed }
    private QuestState questState = QuestState.NotStarted;

    private void Start()
    {
        dialogueUI = DialogueController.Instance;
    }

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        if (dialogueData == null || (PauseController.IsGamePaused && !isDialogueActive))
            return;

        if (isDialogueActive)
        {
            NextLine();
        }
        else
        {
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        Debug.Log("🗨️ StartDialogue() - เริ่ม Dialogue กับ NPC");

        //Sync with quest data
        SyncQuestState();

        //Set dialogue line based on questState
        if (questState == QuestState.NotStarted)
        {
            Debug.Log("   📋 สถานะ: ยังไม่ได้รับเควส");
            dialogueIndex = 0;
        }
        else if (questState == QuestState.InProgress)
        {
            Debug.Log("   ⏳ สถานะ: เควสกำลังดำเนินการ");
            dialogueIndex = dialogueData.questInProgressIndex;
        }
        else if (questState == QuestState.Completed)
        {
            Debug.Log("   ✅ สถานะ: เควสเสร็จแล้ว!");
            dialogueIndex = dialogueData.questCompletedIndex;

            // ✅ ส่งเควสทันทีเมื่อเควสเสร็จ
            TurnInQuest();
        }

        isDialogueActive = true;

        dialogueUI.SetNPCInfo(dialogueData.npcName, dialogueData.npcPortrait);
        dialogueUI.ShowDialogueUI(true);
        PauseController.SetPause(true);

        DisplayCurrentLine();
    }

    /// <summary>
    /// ✅ ส่งเควส
    /// </summary>
    void TurnInQuest()
    {
        if (dialogueData.quest == null)
        {
            Debug.LogWarning("⚠️ NPC ไม่มีเควส");
            return;
        }

        string questID = dialogueData.quest.questID;
        QuestProgress questProgress = QuestController.Instance.GetQuestProgress(questID);

        if (questProgress != null && questProgress.IsCompleted)
        {
            Debug.Log($"📜 ส่งเควส: {dialogueData.quest.questName}");
            QuestController.Instance.TurnInQuest(questProgress);
        }
        else
        {
            Debug.LogWarning("⚠️ ไม่พบเควสหรือเควสยังไม่เสร็จ");
        }
    }

    private void SyncQuestState()
    {
        if (dialogueData.quest == null) return;

        string questID = dialogueData.quest.questID;

        // ✅ เช็คว่าเคยส่งเควสแล้วหรือยัง
        if (QuestController.Instance.IsQuestCompleted(questID))
        {
            Debug.Log("   ✓ เควสนี้เคยส่งแล้ว");
            questState = QuestState.NotStarted; // ให้คุยปกติ
            return;
        }

        if (QuestController.Instance.IsQuestActive(questID))
        {
            // เช็คว่าเควสทำเสร็จหรือยัง
            if (IsQuestCompleted(questID))
            {
                questState = QuestState.Completed;
            }
            else
            {
                questState = QuestState.InProgress;
            }
        }
        else
        {
            questState = QuestState.NotStarted;
        }
    }

    private bool IsQuestCompleted(string questID)
    {
        // หา QuestProgress จาก questID
        QuestProgress questProgress = QuestController.Instance.activateQuests.Find(q => q.quest.questID == questID);

        if (questProgress == null) return false;

        // เช็คว่า objectives ทั้งหมดเสร็จหรือยัง
        foreach (QuestObjective objective in questProgress.objectives)
        {
            if (objective.currentAmount < objective.requiredAmount)
            {
                return false; // ยังทำไม่เสร็จ
            }
        }

        return true; // เสร็จหมดแล้ว
    }

    void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueUI.SetDialogueText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }

        dialogueUI.ClearChoices();

        if (dialogueData.endDialogueLine.Length > dialogueIndex && dialogueData.endDialogueLine[dialogueIndex])
        {
            EndDialogue();
            return;
        }

        foreach (DialogueChoice dialogueChoice in dialogueData.choices)
        {
            if (dialogueChoice.dialogueIndex == dialogueIndex)
            {
                DisplayChoices(dialogueChoice);
                return;
            }
        }

        if (++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            DisplayCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueUI.SetDialogueText("");

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueUI.SetDialogueText(dialogueUI.dialogueText.text += letter);

            if (SoundEffectManager.Instance != null && dialogueData.voiceSound != null)
            {
                SoundEffectManager.Instance.PlayVoice(dialogueData.voiceSound, dialogueData.voicePitch);
            }

            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        isTyping = false;

        if (dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    void DisplayChoices(DialogueChoice choice)
    {
        for (int i = 0; i < choice.choices.Length; i++)
        {
            int nextIndex = choice.nextDialogueIndexes[i];
            bool givesQuest = choice.givesQuest[i];
            dialogueUI.CreateChoiceButton(choice.choices[i], () => ChooseOption(nextIndex, givesQuest));
        }
    }

    void ChooseOption(int nextIndex, bool givesQuest)
    {
        if (givesQuest)
        {
            Debug.Log($"✅ รับเควส: {dialogueData.quest.questName}");
            QuestController.Instance.AcceptQuest(dialogueData.quest);
            questState = QuestState.InProgress;
        }

        dialogueIndex = nextIndex;
        dialogueUI.ClearChoices();
        DisplayCurrentLine();
    }

    void DisplayCurrentLine()
    {
        StopAllCoroutines();
        StartCoroutine(TypeLine());
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueUI.SetDialogueText("");
        dialogueUI.ShowDialogueUI(false);
        PauseController.SetPause(false);
    }
}