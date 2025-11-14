using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestController : MonoBehaviour
{
    public static QuestController Instance { get; private set; }

    [Header("Quest Lists")]
    public List<QuestProgress> activateQuests = new();
    public List<QuestProgress> completedQuests = new(); // ✅ เพิ่มรายการเควสที่เสร็จแล้ว

    private QuestUI questUI;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        questUI = FindObjectOfType<QuestUI>();

        if (InventoryController.Instance != null)
        {
            InventoryController.Instance.OnInventoryChanged += CheckInventoryForQuests;
        }
    }

    public void AcceptQuest(Quest quest)
    {
        if (IsQuestActive(quest.questID))
        {
            Debug.Log($"เควส '{quest.questName}' รับแล้ว!");
            return;
        }

        if (IsQuestCompleted(quest.questID))
        {
            Debug.Log($"เควส '{quest.questName}' ทำเสร็จแล้ว!");
            return;
        }

        activateQuests.Add(new QuestProgress(quest));
        questUI?.UpdateQuestUI();

        Debug.Log($"รับเควส: {quest.questName}");
    }

    public bool IsQuestActive(string questID) => activateQuests.Exists(q => q.quest.questID == questID);

    public bool IsQuestCompleted(string questID) => completedQuests.Exists(q => q.quest.questID == questID);

    public void CheckInventoryForQuests()
    {
        Dictionary<int, int> itemCounts = InventoryController.Instance.GetItemCounts();

        foreach (QuestProgress quest in activateQuests)
        {
            foreach (QuestObjective questObjective in quest.objectives)
            {
                if (questObjective.type != ObjectiveType.CollectItem) continue;
                if (!int.TryParse(questObjective.objectiveID, out int itemID)) continue;

                int newAmount = itemCounts.TryGetValue(itemID, out int count) ? Mathf.Min(count, questObjective.requiredAmount) : 0;

                if (questObjective.currentAmount != newAmount)
                {
                    questObjective.currentAmount = newAmount;
                }
            }
        }

        questUI?.UpdateQuestUI();
    }

    /// <summary>
    /// ✅ ส่งเควส (Turn In Quest)
    /// </summary>
    public void TurnInQuest(QuestProgress questProgress)
    {
        Debug.Log($"🎯 TurnInQuest() ถูกเรียก!");

        if (questProgress == null)
        {
            Debug.LogError("❌ questProgress เป็น null!");
            return;
        }

        Debug.Log($"📜 กำลังส่งเควส: {questProgress.quest.questName}");
        Debug.Log($"   สถานะ: {(questProgress.IsCompleted ? "เสร็จแล้ว ✓" : "ยังไม่เสร็จ ✗")}");

        if (!questProgress.IsCompleted)
        {
            Debug.LogWarning($"⚠️ เควส '{questProgress.quest.questName}' ยังทำไม่เสร็จ!");
            return;
        }

        // ✅ ลบไอเทมที่ต้องส่ง
        Debug.Log("🗑️ กำลังลบไอเทม...");
        RemoveQuestItems(questProgress);

        // ✅ ให้รางวัล
        Debug.Log("🎁 กำลังให้รางวัล...");
        GiveRewards(questProgress);

        // ✅ ย้ายเควสไป Completed
        Debug.Log("✅ กำลังทำเควสให้เสร็จ...");
        CompleteQuest(questProgress);
    }

    /// <summary>
    /// ✅ ส่งเควสตาม ID
    /// </summary>
    public void TurnInQuestByID(string questID)
    {
        QuestProgress quest = activateQuests.Find(q => q.questID == questID);

        if (quest != null)
        {
            TurnInQuest(quest);
        }
        else
        {
            Debug.LogWarning($"⚠️ ไม่พบเควส ID: {questID}");
        }
    }

    /// <summary>
    /// ✅ ลบไอเทมที่ต้องส่ง
    /// </summary>
    void RemoveQuestItems(QuestProgress questProgress)
    {
        Debug.Log($"🔍 RemoveQuestItems() เริ่มทำงาน");

        if (InventoryController.Instance == null)
        {
            Debug.LogError("❌ ไม่พบ InventoryController!");
            return;
        }

        foreach (QuestObjective objective in questProgress.objectives)
        {
            Debug.Log($"   - Objective: {objective.description} (Type: {objective.type})");

            if (objective.type == ObjectiveType.CollectItem)
            {
                Debug.Log($"     ✓ เป็นการเก็บไอเทม objectiveID: {objective.objectiveID}");

                if (int.TryParse(objective.objectiveID, out int itemID))
                {
                    Debug.Log($"     🗑️ พยายามลบไอเทม ID:{itemID} จำนวน {objective.requiredAmount}");

                    // ลบไอเทมจาก Inventory
                    bool success = InventoryController.Instance.RemoveItem(itemID, objective.requiredAmount);

                    if (success)
                    {
                        Debug.Log($"     ✅ ลบสำเร็จ!");
                    }
                    else
                    {
                        Debug.LogError($"     ❌ ลบไม่สำเร็จ!");
                    }
                }
                else
                {
                    Debug.LogError($"     ❌ objectiveID '{objective.objectiveID}' แปลงเป็นตัวเลขไม่ได้!");
                }
            }
        }

        Debug.Log($"✅ RemoveQuestItems() เสร็จสิ้น");
    }

    /// <summary>
    /// ✅ ให้รางวัล
    /// </summary>
    void GiveRewards(QuestProgress questProgress)
    {
        // TODO: เพิ่มระบบรางวัลตามที่ต้องการ
        // เช่น Exp, Gold, Items

        Debug.Log($"🎁 รับรางวัลจากเควส: {questProgress.quest.questName}");

        // ตัวอย่าง:
        // PlayerStats.Instance.AddExp(questProgress.quest.expReward);
        // PlayerStats.Instance.AddGold(questProgress.quest.goldReward);
    }

    /// <summary>
    /// ✅ เสร็จเควส
    /// </summary>
    void CompleteQuest(QuestProgress questProgress)
    {
        Debug.Log($"🎯 CompleteQuest() เริ่มทำงาน");
        Debug.Log($"   เควสที่จะลบ: {questProgress.quest.questName}");
        Debug.Log($"   จำนวนเควส Active ก่อนลบ: {activateQuests.Count}");

        // ลบจาก Active Quests
        bool removed = activateQuests.Remove(questProgress);
        Debug.Log($"   ลบออกจาก Active สำเร็จ: {removed}");
        Debug.Log($"   จำนวนเควส Active หลังลบ: {activateQuests.Count}");

        // เพิ่มเข้า Completed Quests
        completedQuests.Add(questProgress);
        Debug.Log($"   เพิ่มเข้า Completed แล้ว");
        Debug.Log($"   จำนวนเควส Completed: {completedQuests.Count}");

        Debug.Log($"✅ เควส '{questProgress.quest.questName}' เสร็จสมบูรณ์!");

        // อัพเดท UI
        Debug.Log($"🔄 กำลังอัพเดท UI...");
        if (questUI != null)
        {
            questUI.UpdateQuestUI();
            Debug.Log($"✅ อัพเดท UI สำเร็จ");
        }
        else
        {
            Debug.LogError($"❌ ไม่พบ questUI!");
        }

        // บันทึกเกม
        Debug.Log($"💾 กำลังบันทึกเกม...");
        SaveController saveController = FindObjectOfType<SaveController>();
        if (saveController != null)
        {
            saveController.SaveGame();
            Debug.Log($"✅ บันทึกเกมสำเร็จ");
        }
        else
        {
            Debug.LogWarning($"⚠️ ไม่พบ SaveController");
        }

        Debug.Log($"🎉 CompleteQuest() เสร็จสิ้น!");
    }

    /// <summary>
    /// ✅ ยกเลิกเควส (ถ้าต้องการ)
    /// </summary>
    public void AbandonQuest(QuestProgress questProgress)
    {
        activateQuests.Remove(questProgress);
        questUI?.UpdateQuestUI();

        Debug.Log($"❌ ยกเลิกเควส: {questProgress.quest.questName}");
    }

    /// <summary>
    /// ✅ ดูว่าเควสทำเสร็จหรือยัง
    /// </summary>
    public QuestProgress GetQuestProgress(string questID)
    {
        return activateQuests.Find(q => q.questID == questID);
    }

    public void LoadQuestProgress(List<QuestProgressSaveData> savedQuests)
    {
        if (savedQuests == null || savedQuests.Count == 0)
        {
            Debug.Log("No quest progress data found to load.");
            return;
        }

        activateQuests.Clear();

        // โหลด Quest ScriptableObjects ทั้งหมดจาก Resources
        Quest[] allQuests = Resources.LoadAll<Quest>("Quests");

        foreach (QuestProgressSaveData savedQuest in savedQuests)
        {
            // หา Quest จาก questID
            Quest originalQuest = System.Array.Find(allQuests, q => q.questID == savedQuest.questID);

            if (originalQuest == null)
            {
                Debug.LogWarning($"Quest with ID {savedQuest.questID} not found in Resources!");
                continue;
            }

            // สร้าง QuestProgress ใหม่จาก Quest ที่หาเจอ
            QuestProgress newQuest = new QuestProgress(originalQuest);

            // อัปเดต currentAmount จากข้อมูลที่ save ไว้
            for (int i = 0; i < savedQuest.objectives.Count && i < newQuest.objectives.Count; i++)
            {
                newQuest.objectives[i].currentAmount = savedQuest.objectives[i].currentAmount;
            }

            activateQuests.Add(newQuest);
        }

        questUI?.UpdateQuestUI();
        Debug.Log($"Loaded {activateQuests.Count} quests successfully.");
    }

    /// <summary>
    /// ✅ Debug: แสดงเควสทั้งหมด
    /// </summary>
    [ContextMenu("Show All Quests")]
    void ShowAllQuests()
    {
        Debug.Log($"📋 Active Quests: {activateQuests.Count}");
        foreach (var q in activateQuests)
        {
            Debug.Log($"  - {q.quest.questName} ({(q.IsCompleted ? "เสร็จแล้ว ✓" : "ยังไม่เสร็จ ✗")})");
        }

        Debug.Log($"✅ Completed Quests: {completedQuests.Count}");
        foreach (var q in completedQuests)
        {
            Debug.Log($"  - {q.quest.questName}");
        }
    }

    /// <summary>
    /// ✅ Debug: ส่งเควสแรก
    /// </summary>
    [ContextMenu("Test: Turn In First Quest")]
    void TestTurnInFirstQuest()
    {
        if (activateQuests.Count > 0)
        {
            Debug.Log("🧪 ทดสอบส่งเควสแรก...");
            TurnInQuest(activateQuests[0]);
        }
        else
        {
            Debug.LogWarning("⚠️ ไม่มีเควสที่จะส่ง");
        }
    }

    /// <summary>
    /// ✅ Debug: ทำให้เควสเสร็จ
    /// </summary>
    [ContextMenu("Test: Force Complete First Quest")]
    void TestForceCompleteQuest()
    {
        if (activateQuests.Count > 0)
        {
            Debug.Log("🧪 บังคับให้เควสแรกเสร็จ...");
            QuestProgress quest = activateQuests[0];

            // ทำให้ทุก Objective เสร็จ
            foreach (var objective in quest.objectives)
            {
                objective.currentAmount = objective.requiredAmount;
            }

            questUI?.UpdateQuestUI();
            Debug.Log("✅ เควสเสร็จแล้ว! ลองส่งเควสดู");
        }
        else
        {
            Debug.LogWarning("⚠️ ไม่มีเควสที่จะทำให้เสร็จ");
        }
    }
}