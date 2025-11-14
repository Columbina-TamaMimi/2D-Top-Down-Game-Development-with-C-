using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class VisualNovelMenu : MonoBehaviour
{
    [Header("UI References")]
    public Button startButton;
    public Button loadButton;
    public Button settingsButton;
    public Button exitButton;
    public Button backButton;
    public Button closeButton;

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject loadGamePanel;
    public CanvasGroup menuCanvasGroup;

    [Header("Loading")]
    public GameObject loadingCanvas;

    [Header("Settings")]
    public string gameSceneName = "SampleScene";
    public float buttonHoverScale = 1.1f;
    public float fadeInDuration = 1f;
    public float buttonActionDelay = 0.3f; // ⏱️ หน่วงเวลาหลังกดปุ่ม (ให้เสียงเล่นจบ)

    [Header("Audio")]
    public AudioSource bgmSource;
    public AudioClip menuBGM;
    public float exitDelay = 0.5f;

    void Start()
    {
        // ตั้งค่าปุ่ม
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartGame);
            AddButtonHoverEffect(startButton);
        }

        if (loadButton != null)
        {
            loadButton.onClick.AddListener(OnLoadGame);
            AddButtonHoverEffect(loadButton);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnOpenSettings);
            AddButtonHoverEffect(settingsButton);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitGame);
            AddButtonHoverEffect(exitButton);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButton);
            AddButtonHoverEffect(backButton);
        }

        if (closeButton != null)
        {
            backButton.onClick.AddListener(OnBackButton);
            AddButtonHoverEffect(backButton);
        }

        // ซ่อน Panels
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (loadGamePanel != null)
            loadGamePanel.SetActive(false);

        // เล่นเพลงพื้นหลัง
        if (bgmSource != null && menuBGM != null)
        {
            bgmSource.clip = menuBGM;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        // Fade In เมนู
        StartCoroutine(FadeInMenu());
    }

    void Update()
    {
        // กด ESC เพื่อย้อนกลับ
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (loadGamePanel != null && loadGamePanel.activeSelf)
            {
                OnBackButton();
            }
            else if (settingsPanel != null && settingsPanel.activeSelf)
            {
                OnBackButton();
            }
        }
    }

    IEnumerator FadeInMenu()
    {
        if (menuCanvasGroup == null) yield break;

        menuCanvasGroup.alpha = 0;
        float elapsed = 0;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            menuCanvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / fadeInDuration);
            yield return null;
        }

        menuCanvasGroup.alpha = 1;
    }

    void AddButtonHoverEffect(Button button)
    {
        var eventTrigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (eventTrigger == null)
            eventTrigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry
        {
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
        };
        pointerEnter.callback.AddListener((data) => { OnButtonHover(button, true); });
        eventTrigger.triggers.Add(pointerEnter);

        var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry
        {
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit
        };
        pointerExit.callback.AddListener((data) => { OnButtonHover(button, false); });
        eventTrigger.triggers.Add(pointerExit);
    }

    void OnButtonHover(Button button, bool isHovering)
    {
        float targetScale = isHovering ? buttonHoverScale : 1f;
        StartCoroutine(ScaleButton(button.transform, targetScale));
    }

    IEnumerator ScaleButton(Transform buttonTransform, float targetScale)
    {
        Vector3 startScale = buttonTransform.localScale;
        Vector3 endScale = Vector3.one * targetScale;
        float duration = 0.1f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            buttonTransform.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);
            yield return null;
        }

        buttonTransform.localScale = endScale;
    }

    void ShowMainMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }

    void HideMainMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
    }

    // ================== ฟังก์ชันปุ่ม ==================

    public void OnStartGame()
    {
        Debug.Log("🎮 กดปุ่ม START - เริ่มเกมใหม่");

        PlayerPrefs.DeleteKey("CurrentSlot");
        PlayerPrefs.DeleteKey("LoadData");

        if (LoadingScreenManager.Instance != null)
        {
            Debug.Log($"✅ พบ LoadingScreenManager - โหลด Scene: {gameSceneName}");
            LoadingScreenManager.Instance.LoadNewGame(gameSceneName);
        }
        else
        {
            Debug.LogWarning("⚠️ ไม่พบ LoadingScreenManager - โหลดแบบปกติ");
            SceneManager.LoadScene(gameSceneName);
        }
    }

    public void OnLoadGame()
    {
        Debug.Log("💾 กดปุ่ม LOAD");
        // ⏱️ หน่วงเวลาเพื่อให้เสียงของปุ่มเล่นจบก่อน
        StartCoroutine(OpenLoadPanelWithDelay());
    }

    IEnumerator OpenLoadPanelWithDelay()
    {
        // รอให้เสียงเล่นจบ
        yield return new WaitForSeconds(buttonActionDelay);

        // แสดงหน้า Load
        if (loadGamePanel != null)
        {
            loadGamePanel.SetActive(true);
            HideMainMenu();
        }
        else
        {
            Debug.LogWarning("⚠️ ไม่พบ loadGamePanel!");
        }
    }

    public void OnOpenSettings()
    {
        Debug.Log("⚙️ กดปุ่ม SETTINGS");
        StartCoroutine(OpenSettingsWithDelay());
    }

    IEnumerator OpenSettingsWithDelay()
    {
        // รอให้เสียงเล่นจบ
        yield return new WaitForSeconds(buttonActionDelay);

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            HideMainMenu();
        }
    }

    public void OnBackButton()
    {
        Debug.Log("🔙 กดปุ่ม BACK - กลับหน้าเมนู");
        // ⏱️ หน่วงเวลาเพื่อให้เสียงของปุ่มเล่นจบก่อน
        StartCoroutine(BackToMenuWithDelay());
    }

    IEnumerator BackToMenuWithDelay()
    {
        // รอให้เสียงเล่นจบ
        yield return new WaitForSeconds(buttonActionDelay);

        // แสดงหน้าเมนูหลัก
        ShowMainMenu();

        // ซ่อนทุก Panel
        if (loadGamePanel != null)
            loadGamePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void OnExitGame()
    {
        Debug.Log("🚪 กดปุ่ม EXIT");

        // ปิดหน้าโหลด (ถ้ามี)
        if (loadingCanvas != null)
        {
            loadingCanvas.SetActive(false);
            Debug.Log("🧹 ปิดหน้า LoadingCanvas ก่อนออกเกม");
        }

        // หน่วงเวลาออกเกม
        StartCoroutine(ExitAfterDelay());
    }

    IEnumerator ExitAfterDelay()
    {
        yield return new WaitForSeconds(exitDelay);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    IEnumerator LoadSceneWithFade(string sceneName)
    {
        if (menuCanvasGroup != null)
        {
            float elapsed = 0;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                menuCanvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / fadeInDuration);
                yield return null;
            }
        }

        SceneManager.LoadScene(sceneName);
    }

    public static void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}