using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class ExitButton : MonoBehaviour
{
    [Header("Settings")]
    public string menuSceneName = "MainMenu"; // ชื่อ Scene ของหน้าเมนู
    public float delayBeforeExit = 1.5f;       // เวลาหน่วงก่อนเปลี่ยนฉาก

    [Header("UI / Audio Effects")]
    public CanvasGroup fadeCanvas;            // ใส่ CanvasGroup ของภาพดำ (Fade Out)
    public AudioSource clickSound;            // เสียงคลิกปุ่ม

    public void ExitToMenu()
    {
        StartCoroutine(ExitSequence());
    }

    private IEnumerator ExitSequence()
    {
        // เล่นเสียงคลิก
        if (clickSound != null)
            clickSound.Play();

        // ถ้ามี CanvasGroup สำหรับเฟด
        if (fadeCanvas != null)
        {
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime / delayBeforeExit;
                fadeCanvas.alpha = t;
                yield return null;
            }
        }
        else
        {
            // ถ้าไม่มีเฟด ก็รอเฉยๆ
            yield return new WaitForSeconds(delayBeforeExit);
        }

        // โหลดซีนเมนู
        SceneManager.LoadScene(menuSceneName);
    }
}
