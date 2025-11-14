using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ExitConfirmationManager : MonoBehaviour
{
    public static ExitConfirmationManager Instance;

    [Header("UI References")]
    public GameObject confirmationPanel;
    public TextMeshProUGUI messageText;
    public Button saveAndExitButton;
    public Button exitWithoutSaveButton;
    public Button cancelButton;

    [Header("Settings")]
    public string menuSceneName = "MainMenu";
    public float fadeSpeed = 1f;

    [Header("Fade Effect")]
    public CanvasGroup fadeCanvas;

    [Header("Audio")]
    public AudioClip clickSoundClip;
    public AudioClip warningSoundClip;
    [Range(0f, 1f)]
    public float soundVolume = 1f;

    private AudioSource audioSource;
    private bool isExiting = false;
    private bool isPanelOpen = false;

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

    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = soundVolume;
        }

        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }

        if (saveAndExitButton != null)
            saveAndExitButton.onClick.AddListener(OnSaveAndExit);

        if (exitWithoutSaveButton != null)
            exitWithoutSaveButton.onClick.AddListener(OnExitWithoutSave);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancel);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isExiting)
        {
            // ✅ ถ้า Panel เปิดอยู่ → ปิด Panel
            if (isPanelOpen)
            {
                OnCancel();
            }
            // ✅ ถ้า Panel ไม่ได้เปิด → เช็ค Tab
            else
            {
                TabController tabController = FindObjectOfType<TabController>();

                // ✅ ถ้ามี Tab เปิดอยู่ → ห้ามเปิด Confirmation
                if (tabController != null && tabController.IsAnyTabOpen())
                {
                    Debug.Log("[Exit] ❌ มี Tab เปิดอยู่ ไม่สามารถเปิด Confirmation ได้");
                    return;
                }

                // ✅ ถ้าไม่มี Tab เปิด → เปิด Confirmation ได้
                ShowConfirmation();
            }
        }
    }

    // ✅ ฟังก์ชันเช็คว่า Panel เปิดอยู่หรือไม่
    public bool IsPanelOpen()
    {
        return isPanelOpen;
    }

    public void ShowConfirmation()
    {
        if (confirmationPanel == null) return;

        // ✅ เช็คว่ามี Tab เปิดอยู่หรือไม่
        TabController tabController = FindObjectOfType<TabController>();
        if (tabController != null && tabController.IsAnyTabOpen())
        {
            Debug.Log("[Exit] ❌ มี Tab เปิดอยู่ ปิด Tab ก่อนค่อยเปิด Confirmation");
            return;
        }

        isPanelOpen = true;
        confirmationPanel.SetActive(true);
        Time.timeScale = 0f;

        PlaySound(warningSoundClip);

        if (messageText != null)
        {
            messageText.text = "คุณต้องการออกจากเกมหรือไม่?\n\nอย่าลืมบันทึกเกมก่อนออก!";
        }

        Debug.Log("[Exit] แสดง Confirmation Panel");
    }

    void OnSaveAndExit()
    {
        if (isExiting) return;
        isExiting = true;

        PlaySound(clickSoundClip);

        SaveController saveController = FindObjectOfType<SaveController>();
        if (saveController != null)
        {
            saveController.SaveGame();
        }

        StartCoroutine(ExitSequence());
    }

    void OnExitWithoutSave()
    {
        if (isExiting) return;
        isExiting = true;

        PlaySound(clickSoundClip);
        StartCoroutine(ExitSequence());
    }

    void OnCancel()
    {
        isPanelOpen = false;
        confirmationPanel.SetActive(false);
        Time.timeScale = 1f;

        PlaySound(clickSoundClip);

        Debug.Log("[Exit] ปิด Confirmation Panel");
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, soundVolume);
        }
    }

    private IEnumerator ExitSequence()
    {
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);

        Time.timeScale = 1f;

        if (fadeCanvas != null)
        {
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * fadeSpeed;
                fadeCanvas.alpha = t;
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        SceneManager.LoadScene(menuSceneName);
    }

    public static void Show()
    {
        if (Instance != null)
        {
            Instance.ShowConfirmation();
        }
    }
}