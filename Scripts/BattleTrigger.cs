using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleTrigger : MonoBehaviour
{
    public string battleSceneName = "BattleScene"; // ใส่ชื่อ Scene ต่อสู้ที่คุณมี

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // บันทึกตำแหน่งก่อนเข้าสู้
            GameManager.Instance.lastPlayerPosition = other.transform.position;
            GameManager.Instance.lastSceneName = SceneManager.GetActiveScene().name;
            GameManager.Instance.isReturningFromBattle = true; // ✅ เปลี่ยนเป็น true

            Debug.Log("🗡️ เข้าสู้จากซีน: " + GameManager.Instance.lastSceneName +
                      " | ตำแหน่ง: " + GameManager.Instance.lastPlayerPosition);

            // เปลี่ยนไปฉากต่อสู้
            SceneManager.LoadScene(battleSceneName);
        }
    }
}