using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveController : MonoBehaviour
{
    private string saveLocation;
    private InventoryController inventoryController;
    private Chest[] chests;
    private GameObject player;

    void Awake()
    {
        Debug.Log("[DEBUG] 🔥 SaveController Awake() เริ่มทำงาน!");
        InitializeComponents();
    }

    void Start()
    {
        // 🔥 Debug: ดูว่า Player อยู่ตำแหน่งไหนก่อนโหลด
        if (player != null)
        {
            Debug.Log($"[DEBUG] 🎮 Player position BEFORE LoadGame: {player.transform.position}");
        }

        LoadGame();

        // 🔥 Debug: ดูหลังโหลดเสร็จ
        StartCoroutine(CheckPlayerPositionAfterLoad());
    }

    private IEnumerator CheckPlayerPositionAfterLoad()
    {
        yield return new WaitForSeconds(1f);
        if (player != null)
        {
            Debug.Log($"[DEBUG] 🎮 Player position AFTER LoadGame (1 sec): {player.transform.position}");
        }
    }

    private void InitializeComponents()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        Debug.Log($"[DEBUG] 📂 Save Location: {saveLocation}");

        inventoryController = FindObjectOfType<InventoryController>();
        Debug.Log($"[DEBUG] Inventory: {(inventoryController != null ? "Found" : "NOT FOUND")}");

        chests = FindObjectsOfType<Chest>();
        Debug.Log($"[DEBUG] Chests: {chests.Length} found");

        player = GameObject.FindGameObjectWithTag("Player");
        Debug.Log($"[DEBUG] Player: {(player != null ? $"Found at {player.transform.position}" : "NOT FOUND")}");
    }

    public void SaveGame()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        Debug.Log($"[SAVE] 💾 Saving Player Position: {player.transform.position}");

        // แปลง QuestProgress เป็น QuestProgressSaveData
        List<QuestProgressSaveData> questSaveData = new List<QuestProgressSaveData>();
        foreach (var quest in QuestController.Instance.activateQuests)
        {
            QuestProgressSaveData saveData = new QuestProgressSaveData
            {
                questID = quest.quest.questID,
                objectives = new List<QuestObjectiveSaveData>()
            };

            foreach (var objective in quest.objectives)
            {
                saveData.objectives.Add(new QuestObjectiveSaveData
                {
                    type = objective.type,
                    objectiveID = objective.objectiveID,
                    requiredAmount = objective.requiredAmount,
                    currentAmount = objective.currentAmount
                });
            }

            questSaveData.Add(saveData);
        }

        SaveData saveDataFinal = new SaveData
        {
            playerPosition = player.transform.position,
            mapBoundary = FindObjectOfType<CinemachineConfiner>().m_BoundingShape2D.gameObject.name,
            inventorySaveData = inventoryController.GetInventoryItem(),
            chestSaveData = GetChestsState(),
            questProgressData = questSaveData
        };

        string json = JsonUtility.ToJson(saveDataFinal, true);
        File.WriteAllText(saveLocation, json);

        // เคลียร์ข้อมูลชั่วคราวของ Battle
        ClearTempPosition();

        Debug.Log($"[SAVE] ✅ Game saved successfully");
    }

    private List<ChestSaveData> GetChestsState()
    {
        List<ChestSaveData> chestStates = new List<ChestSaveData>();
        foreach (Chest chest in chests)
        {
            ChestSaveData chestSaveData = new ChestSaveData
            {
                chestID = chest.ChestID,
                isOpened = chest.IsOpened
            };
            chestStates.Add(chestSaveData);
        }
        return chestStates;
    }

    public void LoadGame()
    {
        Debug.Log($"[LOAD] === LoadGame Called ===");

        // ✅ เช็คว่ากลับจาก Battle หรือไม่
        bool isReturningFromBattle = (GameManager.Instance != null && GameManager.Instance.isReturningFromBattle);

        Debug.Log($"[LOAD] isReturningFromBattle: {isReturningFromBattle}");

        if (File.Exists(saveLocation))
        {
            string json = File.ReadAllText(saveLocation);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            // 🔥 ถ้ากลับจาก Battle ให้ GameManager จัดการเอง
            if (isReturningFromBattle)
            {
                Debug.Log($"[LOAD] 🎮 กลับจาก Battle - ให้ GameManager จัดการ");

                // โหลดเฉพาะข้อมูลที่ไม่ใช่ตำแหน่ง Player
                inventoryController.SetInventoryItems(saveData.inventorySaveData);
                LoadChestStates(saveData.chestSaveData);
                QuestController.Instance.LoadQuestProgress(saveData.questProgressData);

                // GameManager จะจัดการ MapBoundary และ Player Position เอง
                return;
            }

            // 📂 โหลดเกมปกติ (กด Continue)
            Debug.Log($"[LOAD] 📂 โหลดเกมปกติ - ตำแหน่ง: {saveData.playerPosition}");
            StartCoroutine(LoadGameData(saveData));
        }
        else
        {
            Debug.Log("[LOAD] ⚠️ No save file found. Waiting for manual save...");
            inventoryController.SetInventoryItems(new List<InventorySaveData>());
        }
    }

    /// <summary>
    /// 📂 โหลดข้อมูลเกมทั้งหมด (กด Continue)
    /// </summary>
    private IEnumerator LoadGameData(SaveData saveData)
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        // 🔥 ย้าย Player ก่อน (ก่อนเซ็ต MapBoundary)
        yield return StartCoroutine(SetPlayerPositionImmediate(saveData.playerPosition));

        // รออีก 1 frame
        yield return null;

        // ตั้งค่า MapBoundary หลังจาก Player อยู่ที่ตำแหน่งแล้ว
        PolygonCollider2D savedMapBoundary = GameObject.Find(saveData.mapBoundary)?.GetComponent<PolygonCollider2D>();
        if (savedMapBoundary != null)
        {
            CinemachineConfiner confiner = FindObjectOfType<CinemachineConfiner>();
            if (confiner != null)
            {
                // 🔥 ปิดกล้องก่อนเปลี่ยน MapBoundary
                Camera mainCamera = Camera.main;
                CinemachineBrain brain = mainCamera?.GetComponent<CinemachineBrain>();
                if (brain != null)
                {
                    brain.enabled = false;
                }

                // เปลี่ยน MapBoundary
                confiner.m_BoundingShape2D = savedMapBoundary;
                MapController_Manual.Instance?.HighlightArea(saveData.mapBoundary);
                Debug.Log($"🗺️ โหลด MapBoundary: {saveData.mapBoundary}");

                yield return null;

                // เปิดกล้องกลับมา
                if (brain != null)
                {
                    brain.enabled = true;
                    brain.ManualUpdate();
                }
            }
        }

        // โหลดข้อมูลอื่นๆ
        inventoryController.SetInventoryItems(saveData.inventorySaveData);
        LoadChestStates(saveData.chestSaveData);
        QuestController.Instance.LoadQuestProgress(saveData.questProgressData);

        Debug.Log($"[LOAD] ✅ โหลดเกมเสร็จสมบูรณ์");
    }

    /// <summary>
    /// 📍 ย้าย Player ทันที (ไม่รอ)
    /// </summary>
    private IEnumerator SetPlayerPositionImmediate(Vector3 position)
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        Debug.Log($"[LOAD] 📍 ย้าย Player ไปยัง: {position}");

        // ปิด Rigidbody2D ชั่วคราว
        var rb2d = player.GetComponent<Rigidbody2D>();
        bool hadRigidbody = rb2d != null;

        if (rb2d != null)
        {
            rb2d.velocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
            rb2d.simulated = false;
        }

        yield return null;

        // ตั้งตำแหน่ง
        player.transform.position = position;

        // บังคับกล้องตาม Player ทันที
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.transform.position = new Vector3(
                position.x,
                position.y,
                mainCamera.transform.position.z
            );
        }

        Debug.Log($"[LOAD] ✅ Player อยู่ที่: {player.transform.position}");

        yield return null;

        if (hadRigidbody && rb2d != null)
        {
            rb2d.simulated = true;
        }
    }

    /// <summary>
    /// 📍 ย้าย Player แบบรอ (ใช้สำหรับกรณีพิเศษ)
    /// </summary>
    private IEnumerator SetPlayerPosition(Vector3 position)
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        Debug.Log($"[LOAD] Loading Player Position: {position}");

        var rb2d = player.GetComponent<Rigidbody2D>();
        bool hadRigidbody = rb2d != null;

        if (rb2d != null)
        {
            rb2d.velocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
            rb2d.simulated = false;
        }

        yield return null;

        player.transform.position = position;

        Debug.Log($"[LOAD] Player position set to: {player.transform.position}");

        yield return null;

        if (hadRigidbody && rb2d != null)
        {
            rb2d.simulated = true;
        }

        yield return new WaitForSeconds(0.5f);
        Debug.Log($"[LOAD] Player position after 0.5 sec: {player.transform.position}");
    }

    private void LoadChestStates(List<ChestSaveData> chestStates)
    {
        foreach (Chest chest in chests)
        {
            ChestSaveData chestSaveData = chestStates.FirstOrDefault(c => c.chestID == chest.ChestID);
            if (chestSaveData != null)
            {
                chest.SetOpened(chestSaveData.isOpened);
            }
        }
    }

    /// <summary>
    /// ✅ ยกเลิกการใช้ตำแหน่งชั่วคราว (เรียกเมื่อเซฟเกมปกติ)
    /// </summary>
    public static void ClearTempPosition()
    {
        PlayerPrefs.DeleteKey("TempPlayerX");
        PlayerPrefs.DeleteKey("TempPlayerY");
        PlayerPrefs.DeleteKey("TempPlayerZ");
        PlayerPrefs.DeleteKey("TempLastInputX");
        PlayerPrefs.DeleteKey("TempLastInputY");
        PlayerPrefs.SetInt("IsReturningFromBattle", 0);
        PlayerPrefs.Save();

        Debug.Log($"[BATTLE] ✅ Cleared temp position");
    }

    /// <summary>
    /// 🔥 ลบไฟล์เซฟ (สำหรับ New Game)
    /// </summary>
    public void DeleteSaveFile()
    {
        if (File.Exists(saveLocation))
        {
            File.Delete(saveLocation);
            Debug.Log("[SAVE] 🗑️ ลบไฟล์เซฟสำเร็จ");
        }
        ClearTempPosition();
    }

    /// <summary>
    /// 🔥 ทดสอบ: บังคับโหลดเกม (กด L)
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("[TEST] 🔥 กด L - บังคับโหลดเกม!");
            LoadGame();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (player == null) player = GameObject.FindGameObjectWithTag("Player");
            Debug.Log($"[TEST] 🎮 Player position: {player.transform.position}");
        }
    }

    /// <summary>
    /// ✅ เช็คว่ามีไฟล์เซฟหรือไม่
    /// </summary>
    public bool HasSaveFile()
    {
        return File.Exists(saveLocation);
    }
}