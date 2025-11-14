using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    // ✅ Singleton Pattern
    public static BattleManager Instance;

    [Header("Battle Settings")]
    public float endBattleDelay = 1f;

    void Awake()
    {
        // ตั้งค่า Singleton
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("✅ BattleManager สร้างใหม่");
        }
        else
        {
            Debug.Log("⚠️ BattleManager มีอยู่แล้ว → ทำลายตัวใหม่");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ✅ เรียกจาก BattleSystem เมื่อชนะ
    /// </summary>
    public void WinBattle()
    {
        Debug.Log("🎉 ชนะการต่อสู้!");
        StartCoroutine(EndBattleCoroutine());
    }

    /// <summary>
    /// ✅ เรียกจาก BattleSystem เมื่อแพ้
    /// </summary>
    public void LoseBattle()
    {
        Debug.Log("💀 แพ้การต่อสู้!");

        // ไม่ลบ Enemy เมื่อแพ้
        if (GameManager.Instance != null)
        {
            GameManager.Instance.defeatedEnemyID = null;
        }

        StartCoroutine(EndBattleCoroutine());
    }

    /// <summary>
    /// จบการต่อสู้ทั่วไป
    /// </summary>
    public void EndBattle()
    {
        Debug.Log("⚔️ จบการต่อสู้");
        StartCoroutine(EndBattleCoroutine());
    }

    /// <summary>
    /// Coroutine กลับ Scene เดิม
    /// </summary>
    private IEnumerator EndBattleCoroutine()
    {
        Debug.Log($"[BATTLE] 🔙 กำลังกลับไปยัง Scene...");

        // หน่วงเวลาให้เห็น Animation หรือข้อความ
        yield return new WaitForSeconds(endBattleDelay);

        // กลับไปยังฉากเดิม
        if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.lastSceneName))
        {
            Debug.Log($"[BATTLE] 🔙 กลับไปยัง Scene: {GameManager.Instance.lastSceneName}");
            SceneManager.LoadScene(GameManager.Instance.lastSceneName);
        }
        else
        {
            Debug.LogError("❌ ไม่พบ GameManager หรือชื่อ Scene!");
            // Fallback: กลับไปฉากหลัก
            SceneManager.LoadScene("SampleScene");
        }
    }

    /// <summary>
    /// หนีจากการต่อสู้
    /// </summary>
    public void EscapeBattle()
    {
        Debug.Log("🏃 หนีจากการต่อสู้");

        // ไม่ลบ Enemy เมื่อหนี
        if (GameManager.Instance != null)
        {
            GameManager.Instance.defeatedEnemyID = null;
        }

        StartCoroutine(EndBattleCoroutine());
    }

    /// <summary>
    /// 🎮 กด ESC เพื่อหนีจากการต่อสู้
    /// </summary>
    void Update()
    {
        // กด ESC เพื่อหนีจากการต่อสู้
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("[BATTLE] 🏃 กด ESC - หนีจากการต่อสู้!");
            EscapeBattle();
        }
    }

    /// <summary>
    /// ทดสอบ - กลับทันที (สำหรับ Debug)
    /// </summary>
    [ContextMenu("Test: Return Immediately")]
    public void TestReturnImmediately()
    {
        if (GameManager.Instance != null)
        {
            SceneManager.LoadScene(GameManager.Instance.lastSceneName);
        }
    }
}