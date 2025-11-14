using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// เล่นเสียงเมื่อกดปุ่ม (แบบง่าย)
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour
{
    [Header("Sound Settings")]
    public AudioClip clickSound; // ไฟล์เสียงตอนกด
    public AudioClip hoverSound; // ไฟล์เสียงตอน Hover (optional)

    [Range(0f, 1f)]
    public float volume = 1f; // ระดับเสียง

    private Button button;
    private AudioSource audioSource;

    void Start()
    {
        // หา Button Component
        button = GetComponent<Button>();

        // สร้าง AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = volume;

        // เพิ่ม Listener ให้ปุ่ม
        if (button != null)
        {
            button.onClick.AddListener(PlayClickSound);
        }

        // เพิ่ม Hover Effect (optional)
        AddHoverEffect();
    }

    /// <summary>
    /// เล่นเสียงตอนกดปุ่ม
    /// </summary>
    public void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound, volume);
            Debug.Log($"🔊 เล่นเสียง: {clickSound.name}");
        }
    }

    /// <summary>
    /// เล่นเสียงตอน Hover
    /// </summary>
    public void PlayHoverSound()
    {
        if (hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound, volume * 0.7f);
        }
    }

    /// <summary>
    /// เพิ่ม Hover Effect
    /// </summary>
    void AddHoverEffect()
    {
        var eventTrigger = GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (eventTrigger == null)
            eventTrigger = gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        // Event Hover
        var hoverEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        hoverEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        hoverEntry.callback.AddListener((data) => { PlayHoverSound(); });
        eventTrigger.triggers.Add(hoverEntry);
    }
}