using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Return Data")]
    public Vector3 lastPlayerPosition;
    public string lastSceneName;
    public string lastMapBoundary; // 🔥 เพิ่มบรรทัดนี้
    public bool isReturningFromBattle;
    public string defeatedEnemyID;

    private void Awake()
    {
        Debug.Log("✅ GameManager Awake");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ GameManager สร้างใหม่");
        }
        else
        {
            Debug.Log("⚠️ GameManager มีอยู่แล้ว → ทำลายตัวใหม่");
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"📦 โหลดซีนใหม่: {scene.name} | isReturningFromBattle={isReturningFromBattle}");

        // ถ้ากลับจากต่อสู้ และไม่ใช่ Battle Scene
        if (isReturningFromBattle && scene.name != "BattleScene")
        {
            StartCoroutine(RestorePlayerPosition());
        }
    }

    private IEnumerator RestorePlayerPosition()
    {
        // 🔥 รอให้ Scene โหลดเสร็จสมบูรณ์
        yield return new WaitForEndOfFrame();
        yield return null;

        // 🗺️ กู้คืน MapBoundary ที่ถูกต้อง
        if (!string.IsNullOrEmpty(lastMapBoundary))
        {
            RestoreMapBoundary();
        }

        // ลบ Enemy ที่แพ้
        if (!string.IsNullOrEmpty(defeatedEnemyID))
        {
            GameObject enemy = GameObject.Find(defeatedEnemyID);
            if (enemy != null)
            {
                Destroy(enemy);
                Debug.Log($"🗑️ ลบ Enemy: {defeatedEnemyID}");
            }
            defeatedEnemyID = null;
        }

        // ย้าย Player กลับตำแหน่งเดิม
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // 🎯 บังคับปิด Rigidbody2D ก่อนเทเลพอร์ต
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.simulated = false;
            }

            // ย้ายตำแหน่ง
            player.transform.position = lastPlayerPosition;
            Debug.Log($"✅ ย้าย Player กลับตำแหน่ง: {lastPlayerPosition} (Map: {lastMapBoundary})");

            // 🎥 แก้ไขกล้อง Cinemachine
            yield return StartCoroutine(FixCinemachinePosition(player));

            // เปิด Rigidbody2D กลับมา
            if (rb != null)
            {
                rb.simulated = true;
            }

            // รีเซ็ต Animator
            Animator animator = player.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetBool("isWalking", false);

                // โหลดทิศทางที่บันทึกไว้
                if (PlayerPrefs.HasKey("LastInputX"))
                {
                    animator.SetFloat("LastInputX", PlayerPrefs.GetFloat("LastInputX"));
                    animator.SetFloat("LastInputY", PlayerPrefs.GetFloat("LastInputY"));
                }
            }

            // เปิดให้ Player เดินได้
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.SetCanMove(true);
            }
        }
        else
        {
            Debug.LogWarning("⚠️ ไม่พบ Player!");
        }

        // รีเซ็ตสถานะ
        isReturningFromBattle = false;
        Debug.Log("✅ เสร็จสิ้นการกลับจากต่อสู้");
    }

    /// <summary>
    /// 🗺️ กู้คืน MapBoundary ที่ถูกต้อง
    /// </summary>
    private void RestoreMapBoundary()
    {
        // หา MapBoundary GameObject
        GameObject mapBoundaryObj = GameObject.Find(lastMapBoundary);
        if (mapBoundaryObj == null)
        {
            Debug.LogError($"❌ ไม่พบ MapBoundary: {lastMapBoundary}");
            return;
        }

        PolygonCollider2D mapCollider = mapBoundaryObj.GetComponent<PolygonCollider2D>();
        if (mapCollider == null)
        {
            Debug.LogError($"❌ {lastMapBoundary} ไม่มี PolygonCollider2D!");
            return;
        }

        // ตั้งค่า Cinemachine Confiner
        CinemachineConfiner confiner = FindObjectOfType<CinemachineConfiner>();
        if (confiner != null)
        {
            confiner.m_BoundingShape2D = mapCollider;
            Debug.Log($"🗺️ ตั้งค่า MapBoundary: {lastMapBoundary}");
        }

        // อัพเดท Minimap (ถ้ามี)
        MapController_Manual.Instance?.HighlightArea(lastMapBoundary);
    }

    /// <summary>
    /// 🎥 แก้ไขปัญหากล้อง Cinemachine ไม่ตาม Player
    /// </summary>
    private IEnumerator FixCinemachinePosition(GameObject player)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("⚠️ ไม่พบ Main Camera!");
            yield break;
        }

        CinemachineBrain brain = mainCamera.GetComponent<CinemachineBrain>();
        if (brain == null)
        {
            Debug.LogWarning("⚠️ Main Camera ไม่มี CinemachineBrain!");
            yield break;
        }

        CinemachineVirtualCamera vcam = FindObjectOfType<CinemachineVirtualCamera>();
        if (vcam == null)
        {
            Debug.LogWarning("⚠️ ไม่พบ CinemachineVirtualCamera!");
            yield break;
        }

        // ปิด Brain ชั่วคราว
        brain.enabled = false;

        // บังคับให้กล้องอยู่ตำแหน่ง Player ทันที
        mainCamera.transform.position = new Vector3(
            lastPlayerPosition.x,
            lastPlayerPosition.y,
            mainCamera.transform.position.z
        );

        // บอก Virtual Camera ว่า Target เทเลพอร์ต
        vcam.OnTargetObjectWarped(player.transform, player.transform.position - vcam.transform.position);

        yield return null;

        // เปิด Brain กลับมา
        brain.enabled = true;
        brain.ManualUpdate();

        Debug.Log($"📷 แก้ไขกล้อง Cinemachine สำเร็จ! ตำแหน่ง: {lastPlayerPosition}");
    }

    /// <summary>
    /// เรียกใช้เมื่อต้องการรีเซ็ต GameManager (สำหรับ Debug)
    /// </summary>
    public void ResetGameManager()
    {
        lastPlayerPosition = Vector3.zero;
        lastSceneName = "";
        lastMapBoundary = "";
        isReturningFromBattle = false;
        defeatedEnemyID = null;
        Debug.Log("🔄 รีเซ็ต GameManager");
    }
}