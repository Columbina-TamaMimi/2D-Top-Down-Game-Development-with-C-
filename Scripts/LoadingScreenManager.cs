using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ระบบ Loading Screen พร้อม Fade และ Progress Bar
/// </summary>
public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance { get; private set; }

    [Header("Loading UI")]
    public GameObject loadingPanel; // Panel Loading
    public CanvasGroup loadingCanvasGroup; // สำหรับ Fade
    public Image loadingBar; // Progress Bar (Fill)
    public TextMeshProUGUI loadingText; // ข้อความ "Loading..."
    public TextMeshProUGUI percentText; // เปอร์เซ็นต์ "50%"

    [Header("Settings")]
    public float fadeSpeed = 2f; // ความเร็ว Fade
    public float minimumLoadTime = 1.5f; // เวลาโหลดขั้นต่ำ (เพื่อให้เห็น Loading)
    public bool showFakeProgress = true; // แสดง Progress แบบปลอม (สวยงาม)

    [Header("Loading Tips (Optional)")]
    public string[] loadingTips; // ข้อความ Tips ตอนโหลด
    public TextMeshProUGUI tipText; // Text แสดง Tip

    private bool isLoading = false;

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // ซ่อน Loading Panel ตอนเริ่มต้น
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    #region Load Scene Functions

    /// <summary>
    /// โหลด Scene พร้อม Loading Screen
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (!isLoading)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }
    }

    /// <summary>
    /// โหลด Scene ด้วย Async
    /// </summary>
    IEnumerator LoadSceneAsync(string sceneName)
    {
        isLoading = true;

        // แสดง Loading Panel
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        // Fade In
        yield return StartCoroutine(FadeLoadingScreen(1f));

        // แสดง Tip แบบสุ่ม
        ShowRandomTip();

        // เริ่มโหลด Scene
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false; // ไม่ให้เปลี่ยน Scene ทันที

        float fakeProgress = 0f;
        float elapsedTime = 0f;

        // แสดง Progress
        while (!operation.isDone)
        {
            elapsedTime += Time.deltaTime;

            // Progress จริงจาก Unity (0-0.9)
            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);

            if (showFakeProgress)
            {
                // Fake Progress (เรียบขึ้น)
                fakeProgress = Mathf.Lerp(fakeProgress, realProgress, Time.deltaTime * 3f);
            }
            else
            {
                fakeProgress = realProgress;
            }

            // อัพเดท UI
            UpdateLoadingUI(fakeProgress);

            // ถ้าโหลดเสร็จแล้ว และเวลาผ่านไปพอ
            if (operation.progress >= 0.9f && elapsedTime >= minimumLoadTime)
            {
                // รอให้ Progress Bar เต็ม
                yield return StartCoroutine(FillProgressBar());

                // เปลี่ยน Scene
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        // Fade Out
        yield return StartCoroutine(FadeLoadingScreen(0f));

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false); // ซ่อนแน่ ๆ
        }

        // Option: รีเซ็ต CanvasGroup
        if (loadingCanvasGroup != null)
            loadingCanvasGroup.alpha = 0f;
    }

    /// <summary>
    /// เติม Progress Bar ให้เต็ม
    /// </summary>
    IEnumerator FillProgressBar()
    {
        float progress = loadingBar != null ? loadingBar.fillAmount : 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime * 2f;
            UpdateLoadingUI(progress);
            yield return null;
        }

        UpdateLoadingUI(1f);
    }

    #endregion

    #region Fade Functions

    /// <summary>
    /// Fade Loading Screen (0=ซ่อน, 1=แสดง)
    /// </summary>
    IEnumerator FadeLoadingScreen(float targetAlpha)
    {
        if (loadingCanvasGroup == null) yield break;

        float startAlpha = loadingCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * fadeSpeed;
            loadingCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed);
            yield return null;
        }

        loadingCanvasGroup.alpha = targetAlpha;
    }

    #endregion

    #region UI Update Functions

    /// <summary>
    /// อัพเดท UI Loading
    /// </summary>
    void UpdateLoadingUI(float progress)
    {
        progress = Mathf.Clamp01(progress);

        // Progress Bar
        if (loadingBar != null)
            loadingBar.fillAmount = progress;

        // เปอร์เซ็นต์
        if (percentText != null)
            percentText.text = $"{Mathf.RoundToInt(progress * 100)}%";

        // ข้อความ Loading
        if (loadingText != null)
        {
            int dots = Mathf.FloorToInt(Time.time * 2f) % 4;
            loadingText.text = "Loading" + new string('.', dots);
        }
    }

    /// <summary>
    /// แสดง Tip แบบสุ่ม
    /// </summary>
    void ShowRandomTip()
    {
        if (tipText != null && loadingTips != null && loadingTips.Length > 0)
        {
            string tip = loadingTips[Random.Range(0, loadingTips.Length)];
            tipText.text = tip;
        }
    }

    #endregion

    #region Quick Load Functions

    /// <summary>
    /// โหลดเกมใหม่
    /// </summary>
    public void LoadNewGame(string gameSceneName = "SampleScene")
    {
        LoadScene(gameSceneName);
    }

    /// <summary>
    /// กลับหน้าเมนู
    /// </summary>
    public void LoadMainMenu(string menuSceneName = "MainMenu")
    {
        LoadScene(menuSceneName);
    }

    /// <summary>
    /// รีโหลด Scene ปัจจุบัน
    /// </summary>
    public void ReloadCurrentScene()
    {
        LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// ออกจากเกม (พร้อม Fade)
    /// </summary>
    public void QuitGame()
    {
        StartCoroutine(QuitWithFade());
    }

    IEnumerator QuitWithFade()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        yield return StartCoroutine(FadeLoadingScreen(1f));

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    #endregion
}