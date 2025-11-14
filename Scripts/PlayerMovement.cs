using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    [Header("Battle Settings")]
    [SerializeField] private string battleSceneName = "BattleScene";

    [Header("Footstep Sound Settings")]
    [SerializeField] private string footstepSoundGroup = "Footstep";
    [SerializeField] private float footstepInterval = 0.5f;
    [SerializeField] private float minimumSpeed = 0.1f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private bool canMove = true;

    private float footstepTimer = 0f;
    private bool isMoving = false;
    private bool wasMoving = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (rb == null)
            Debug.LogWarning("Player ไม่มี Rigidbody2D!");
        if (animator == null)
            Debug.LogWarning("Player ไม่มี Animator!");
    }

    void Update()
    {
        if (PauseController.IsGamePaused)
        {
            rb.velocity = Vector2.zero;
            animator.SetBool("isWalking", false);
            isMoving = false;
            wasMoving = false;
            return;
        }

        rb.velocity = moveInput * moveSpeed;
        animator.SetBool("isWalking", rb.velocity.magnitude > 0);

        CheckFootstepSound();
    }

    void CheckFootstepSound()
    {
        isMoving = rb.velocity.magnitude > minimumSpeed && canMove;

        if (isMoving)
        {
            if (!wasMoving)
            {
                PlayFootstep();
                footstepTimer = 0f;
                wasMoving = true;
            }
            else
            {
                footstepTimer += Time.deltaTime;

                if (footstepTimer >= footstepInterval)
                {
                    PlayFootstep();
                    footstepTimer = 0f;
                }
            }
        }
        else
        {
            footstepTimer = 0f;
            wasMoving = false;
        }
    }

    void PlayFootstep()
    {
        if (SoundEffectManager.Instance != null)
        {
            SoundEffectManager.Instance.PlaySoundFromGroup(footstepSoundGroup);
        }
        else
        {
            Debug.LogWarning("⚠️ ไม่พบ SoundEffectManager!");
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            animator.SetBool("isWalking", false);
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }
        moveInput = context.ReadValue<Vector2>();
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("ชนกับ: " + other.name + " Tag: " + other.tag);

        if (other.CompareTag("Enemy"))
        {
            Debug.Log("เริ่มต่อสู้กับ: " + other.name);
            StartBattle(other.gameObject);
        }
        else
        {
            Debug.Log("Tag ไม่ตรง - ต้องเป็น Enemy");
        }
    }

    void StartBattle(GameObject enemy)
    {
        canMove = false;
        rb.velocity = Vector2.zero;
        moveInput = Vector2.zero;
        animator.SetBool("isWalking", false);
        isMoving = false;
        wasMoving = false;

        SaveTemporaryBattleData(enemy);

        Debug.Log("เริ่มต่อสู้กับ: " + enemy.name);

        StartCoroutine(LoadBattleScene());
    }

    void SaveTemporaryBattleData(GameObject enemy)
    {
        // บันทึกใน GameManager (RAM เท่านั้น)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.lastPlayerPosition = transform.position;
            GameManager.Instance.lastSceneName = SceneManager.GetActiveScene().name;
            GameManager.Instance.isReturningFromBattle = true;
            GameManager.Instance.defeatedEnemyID = enemy.name;

            // 🔥 บันทึก MapBoundary ที่กำลังอยู่
            CinemachineConfiner confiner = FindObjectOfType<CinemachineConfiner>();
            if (confiner != null && confiner.m_BoundingShape2D != null)
            {
                GameManager.Instance.lastMapBoundary = confiner.m_BoundingShape2D.gameObject.name;
                Debug.Log($"💾 [RAM] บันทึก MapBoundary: {GameManager.Instance.lastMapBoundary}");
            }
            else
            {
                Debug.LogWarning("⚠️ ไม่พบ CinemachineConfiner หรือ MapBoundary!");
            }

            Debug.Log($"💾 [RAM] บันทึกตำแหน่งสำหรับกลับจากต่อสู้: {transform.position}");
        }

        // บันทึก Enemy Data สำหรับ Battle Scene
        Unit enemyUnit = enemy.GetComponent<Unit>();
        if (enemyUnit != null)
        {
            PlayerPrefs.SetString("TempEnemyName", enemyUnit.unitName);
            PlayerPrefs.SetInt("TempEnemyHP", enemyUnit.maxHP);
            PlayerPrefs.SetInt("TempEnemyAttack", enemyUnit.damage);
            PlayerPrefs.SetInt("TempEnemyLevel", enemyUnit.unitLevel);
        }
        else
        {
            PlayerPrefs.SetString("TempEnemyName", enemy.name);
            PlayerPrefs.SetInt("TempEnemyHP", 100);
            PlayerPrefs.SetInt("TempEnemyAttack", 20);
            PlayerPrefs.SetInt("TempEnemyLevel", 1);
        }

        // บันทึกทิศทางที่หันอยู่
        if (animator != null)
        {
            PlayerPrefs.SetFloat("TempLastInputX", animator.GetFloat("LastInputX"));
            PlayerPrefs.SetFloat("TempLastInputY", animator.GetFloat("LastInputY"));
        }

        PlayerPrefs.Save();
        Debug.Log("💾 บันทึกข้อมูล Enemy ชั่วคราว (ไม่เซฟเกม)");
    }

    IEnumerator LoadBattleScene()
    {
        Debug.Log("กำลังโหลด Battle Scene: " + battleSceneName);

        yield return new WaitForSeconds(0.3f);

        SceneManager.LoadScene(battleSceneName);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"📦 PlayerMovement: โหลดซีน {scene.name}");

        if (scene.name != battleSceneName)
        {
            if (GameManager.Instance != null && GameManager.Instance.isReturningFromBattle)
            {
                Debug.Log("⏳ รอ GameManager จัดการตำแหน่ง...");
            }
            else if (PlayerPrefs.HasKey("PlayerX") && rb != null && animator != null)
            {
                LoadPlayerPosition();
            }
        }
    }

    void LoadPlayerPosition()
    {
        float x = PlayerPrefs.GetFloat("PlayerX");
        float y = PlayerPrefs.GetFloat("PlayerY");
        float z = PlayerPrefs.GetFloat("PlayerZ");

        transform.position = new Vector3(x, y, z);

        float lastX = PlayerPrefs.GetFloat("LastInputX");
        float lastY = PlayerPrefs.GetFloat("LastInputY");
        animator.SetFloat("LastInputX", lastX);
        animator.SetFloat("LastInputY", lastY);

        canMove = true;
        Debug.Log($"📂 โหลดตำแหน่งจากเซฟ: {transform.position}");
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
        if (!canMove)
        {
            rb.velocity = Vector2.zero;
            moveInput = Vector2.zero;
            animator.SetBool("isWalking", false);
            isMoving = false;
            wasMoving = false;
        }
    }
}