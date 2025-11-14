using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [Header("Canvas References")]
    public GameObject mainMenuCanvas;
    public GameObject settingCanvas;

    void Start()
    {
        // เปิดเมนูหลัก ปิดหน้าตั้งค่าตอนเริ่มเกม
        ShowMainMenu();
    }

    // ฟังก์ชันสำหรับปุ่ม Start
    public void StartGame()
    {
        // โหลดฉากเกม
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    // ฟังก์ชันสำหรับปุ่ม Settings
    public void OpenSettings()
    {
        mainMenuCanvas.SetActive(false);
        settingCanvas.SetActive(true);
    }

    // ฟังก์ชันสำหรับปุ่มกลับจากหน้าตั้งค่า
    public void CloseSettings()
    {
        settingCanvas.SetActive(false);
        mainMenuCanvas.SetActive(true);
    }

    // ฟังก์ชันสำหรับปุ่ม Exit
    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void ShowMainMenu()
    {
        mainMenuCanvas.SetActive(true);
        settingCanvas.SetActive(false);
    }
}