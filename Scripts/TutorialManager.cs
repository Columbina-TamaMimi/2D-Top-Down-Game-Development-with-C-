using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject tutorialPanel;
    public GameObject mainMenuPanel;
    public CanvasGroup tutorialCanvasGroup;

    [Header("Tutorial Pages")]
    public GameObject[] tutorialPages;
    private int currentPageIndex = 0;

    [Header("Buttons")]
    public Button nextButton;
    public Button skipButton;
    public Button startButton;
    public Button prevButton; // ปุ่มย้อนกลับ

    [Header("Page Indicators (Optional)")]
    public GameObject[] pageIndicators;
    public Color activeIndicatorColor = Color.white;
    public Color inactiveIndicatorColor = Color.gray;

    [Header("Settings")]
    public bool showOnlyFirstTime = true;
    public float fadeSpeed = 2f;
    public string tutorialSeenKey = "TutorialSeen";

    [Header("Animation")]
    public bool useSlideAnimation = true;
    public float slideSpeed = 0.3f;
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Sound Effects")]
    public AudioClip pageFlipSound;
    public AudioClip completeSound;
    private AudioSource audioSource;

    [Header("Transition Panel")]
    public CanvasGroup fadePanel;
    public float fadePanelSpeed = 2f;

    [Header("Button Hover Effect")]
    public float hoverScale = 1.1f;
    public float hoverSpeed = 8f;

    private Button[] allButtons;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        // รวมปุ่มทั้งหมดเพื่อใส่ Hover Effect
        allButtons = new Button[] { nextButton, prevButton, skipButton, startButton };
        foreach (var btn in allButtons)
        {
            if (btn != null)
            {
                ButtonHoverEffect(btn.gameObject);
            }
        }

        if (showOnlyFirstTime && PlayerPrefs.GetInt(tutorialSeenKey, 0) == 1)
        {
            ShowMainMenu();
            return;
        }

        ShowTutorial();

        if (nextButton != null)
            nextButton.onClick.AddListener(NextPage);

        if (prevButton != null)
            prevButton.onClick.AddListener(PrevPage);

        if (skipButton != null)
            skipButton.onClick.AddListener(() => StartCoroutine(ShowFadePanel(SkipTutorial)));

        if (startButton != null)
            startButton.onClick.AddListener(() => StartCoroutine(ShowFadePanel(CompleteTutorial)));
    }

    // ฟังก์ชันเพิ่ม Hover Effect ให้ปุ่ม
    void ButtonHoverEffect(GameObject buttonObj)
    {
        ButtonHover hover = buttonObj.AddComponent<ButtonHover>();
        hover.scaleUpSize = hoverScale;
        hover.scaleSpeed = hoverSpeed;
    }

    // ================= Button Hover Inner Class =================
    private class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public float scaleUpSize = 1.1f;
        public float scaleSpeed = 8f;
        private Vector3 originalScale;
        private bool isHovered = false;

        void Start()
        {
            originalScale = transform.localScale;
        }

        void Update()
        {
            Vector3 targetScale = isHovered ? originalScale * scaleUpSize : originalScale;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
        }
    }

    // ================= Tutorial Methods =================
    void ShowTutorial()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(true);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);

        ShowPage(0);

        if (tutorialCanvasGroup != null)
            StartCoroutine(FadeCanvas(tutorialCanvasGroup, 1f));

        Debug.Log("📖 แสดงหน้าแนะนำการเล่น");
    }

    void NextPage()
    {
        if (tutorialPages == null || tutorialPages.Length == 0)
        {
            CompleteTutorial();
            return;
        }

        PlaySound(pageFlipSound);
        currentPageIndex++;

        if (currentPageIndex >= tutorialPages.Length)
        {
            StartCoroutine(ShowFadePanel(CompleteTutorial));
        }
        else
        {
            if (useSlideAnimation)
                StartCoroutine(SlideToPage(currentPageIndex, true));
            else
                ShowPage(currentPageIndex);
        }
    }

    void PrevPage()
    {
        if (currentPageIndex <= 0) return;

        PlaySound(pageFlipSound);
        currentPageIndex--;

        if (useSlideAnimation)
            StartCoroutine(SlideToPage(currentPageIndex, false));
        else
            ShowPage(currentPageIndex);
    }

    void ShowPage(int pageIndex)
    {
        if (tutorialPages == null || tutorialPages.Length == 0) return;

        foreach (var page in tutorialPages)
            if (page != null) page.SetActive(false);

        if (pageIndex >= 0 && pageIndex < tutorialPages.Length)
            tutorialPages[pageIndex].SetActive(true);

        UpdateButtons(pageIndex);
        UpdatePageIndicators(pageIndex);
    }

    IEnumerator SlideToPage(int targetPage, bool slideRight)
    {
        if (tutorialPages == null || targetPage < 0 || targetPage >= tutorialPages.Length) yield break;

        GameObject currentPageObj = tutorialPages[currentPageIndex == targetPage ? currentPageIndex : currentPageIndex - (slideRight ? 1 : -1)];
        GameObject nextPageObj = tutorialPages[targetPage];

        RectTransform currentRect = currentPageObj.GetComponent<RectTransform>();
        RectTransform nextRect = nextPageObj.GetComponent<RectTransform>();

        if (currentRect == null || nextRect == null) yield break;

        float screenWidth = GetComponent<RectTransform>().rect.width;
        Vector2 currentStart = currentRect.anchoredPosition;
        Vector2 nextStart = new Vector2(slideRight ? screenWidth : -screenWidth, 0);

        nextRect.anchoredPosition = nextStart;
        nextPageObj.SetActive(true);

        float elapsed = 0f;

        while (elapsed < slideSpeed)
        {
            elapsed += Time.deltaTime;
            float t = slideCurve.Evaluate(elapsed / slideSpeed);

            currentRect.anchoredPosition = Vector2.Lerp(currentStart, new Vector2(slideRight ? -screenWidth : screenWidth, 0), t);
            nextRect.anchoredPosition = Vector2.Lerp(nextStart, Vector2.zero, t);

            yield return null;
        }

        currentPageObj.SetActive(false);
        currentRect.anchoredPosition = currentStart;
        nextRect.anchoredPosition = Vector2.zero;

        UpdateButtons(targetPage);
        UpdatePageIndicators(targetPage);
    }

    void UpdateButtons(int pageIndex)
    {
        bool isFirstPage = (pageIndex <= 0);
        bool isLastPage = (pageIndex >= tutorialPages.Length - 1);

        if (prevButton != null) prevButton.gameObject.SetActive(!isFirstPage);
        if (nextButton != null) nextButton.gameObject.SetActive(!isLastPage);
        if (startButton != null) startButton.gameObject.SetActive(isLastPage);
    }

    void UpdatePageIndicators(int pageIndex)
    {
        if (pageIndicators == null || pageIndicators.Length == 0) return;

        for (int i = 0; i < pageIndicators.Length; i++)
        {
            if (pageIndicators[i] != null)
            {
                Image img = pageIndicators[i].GetComponent<Image>();
                if (img != null) img.color = (i == pageIndex) ? activeIndicatorColor : inactiveIndicatorColor;
            }
        }
    }

    void SkipTutorial()
    {
        Debug.Log("⏩ ข้าม Tutorial");
        CompleteTutorial();
    }

    void CompleteTutorial()
    {
        PlaySound(completeSound);
        PlayerPrefs.SetInt(tutorialSeenKey, 1);
        PlayerPrefs.Save();
        Debug.Log("✅ จบ Tutorial");

        StartCoroutine(FadeOutAndShowMenu());
    }

    IEnumerator FadeOutAndShowMenu()
    {
        if (tutorialCanvasGroup != null)
            yield return StartCoroutine(FadeCanvas(tutorialCanvasGroup, 0f));

        ShowMainMenu();
    }

    void ShowMainMenu()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);

        Debug.Log("🏠 แสดงเมนูหลัก");
    }

    IEnumerator FadeCanvas(CanvasGroup canvasGroup, float targetAlpha)
    {
        if (canvasGroup == null) yield break;

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * fadeSpeed;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    IEnumerator ShowFadePanel(System.Action onComplete)
    {
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            fadePanel.alpha = 0f;

            float elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime * fadePanelSpeed;
                fadePanel.alpha = Mathf.Lerp(0f, 1f, elapsed);
                yield return null;
            }

            fadePanel.alpha = 1f;
            yield return new WaitForSeconds(0.5f);
        }

        onComplete?.Invoke();
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }

    public void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(tutorialSeenKey);
        PlayerPrefs.Save();
        Debug.Log("🔄 รีเซ็ต Tutorial");
    }

    public void ShowTutorialAgain()
    {
        currentPageIndex = 0;
        ShowTutorial();
    }
}
