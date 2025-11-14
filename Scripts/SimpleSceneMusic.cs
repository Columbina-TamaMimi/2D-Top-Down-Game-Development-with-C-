using UnityEngine;

/// <summary>
/// ระบบเพลงแบบง่าย - แต่ละ Scene มี AudioSource เป็นของตัวเอง
/// </summary>
public class SimpleSceneMusic : MonoBehaviour
{
    [Header("🎵 เพลงของ Scene นี้")]
    public AudioClip sceneMusic;

    [Range(0f, 1f)]
    public float volume = 0.5f;

    [Header("Fade Settings")]
    public bool useFade = true;
    public float fadeInDuration = 1f;
    public float fadeOutDuration = 1f;

    private AudioSource audioSource;
    private static SimpleSceneMusic currentMusic; // เก็บ Scene ปัจจุบัน

    void Awake()
    {
        Debug.Log($"[Music] === SimpleSceneMusic Awake ===");
        Debug.Log($"[Music] GameObject: {gameObject.name}");
        Debug.Log($"[Music] sceneMusic: {(sceneMusic != null ? sceneMusic.name : "NULL")}");

        // ✅ เช็ค Audio Listener
        AudioListener listener = FindObjectOfType<AudioListener>();
        if (listener == null)
        {
            Debug.LogError("[Music] ❌ ไม่พบ Audio Listener!");
            return;
        }
        else
        {
            Debug.Log($"[Music] ✅ พบ Audio Listener: {listener.gameObject.name}");
        }

        // ✅ หยุดเพลงเก่า (ถ้ามี)
        if (currentMusic != null && currentMusic != this)
        {
            Debug.Log($"[Music] หยุดเพลงเก่า: {currentMusic.sceneMusic?.name ?? "none"}");

            if (currentMusic.useFade && currentMusic.audioSource != null)
            {
                currentMusic.StartCoroutine(currentMusic.FadeOutAndDestroy());
            }
            else
            {
                Destroy(currentMusic.gameObject);
            }
        }

        currentMusic = this;
        DontDestroyOnLoad(gameObject);

        // ✅ เช็คเพลง
        if (sceneMusic == null)
        {
            Debug.LogError($"[Music] ❌ sceneMusic เป็น null!");
            return;
        }

        // ✅ สร้าง AudioSource
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("[Music] ✅ สร้าง AudioSource ใหม่");
        }

        audioSource.clip = sceneMusic;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = useFade ? 0f : volume;
        audioSource.spatialBlend = 0f; // 2D Sound
        audioSource.priority = 0; // สำคัญที่สุด

        audioSource.Play();

        Debug.Log($"[Music] ✅ เล่นเพลง: {sceneMusic.name}");
        Debug.Log($"[Music] AudioSource.isPlaying: {audioSource.isPlaying}");
        Debug.Log($"[Music] AudioSource.clip: {audioSource.clip?.name ?? "null"}");
        Debug.Log($"[Music] AudioSource.volume: {audioSource.volume}");

        // ✅ Fade In
        if (useFade)
        {
            StartCoroutine(FadeIn());
        }
    }

    System.Collections.IEnumerator FadeIn()
    {
        Debug.Log($"[Music] 🔊 Fade In เริ่ม...");
        float timer = 0f;
        while (timer < fadeInDuration)
        {
            timer += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(0f, volume, timer / fadeInDuration);
            yield return null;
        }
        audioSource.volume = volume;
        Debug.Log($"[Music] ✅ Fade In เสร็จ (volume: {audioSource.volume})");
    }

    System.Collections.IEnumerator FadeOutAndDestroy()
    {
        Debug.Log($"[Music] 🔇 Fade Out เริ่ม...");
        float startVolume = audioSource.volume;
        float timer = 0f;
        while (timer < fadeOutDuration)
        {
            timer += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeOutDuration);
            yield return null;
        }
        audioSource.volume = 0f;
        Debug.Log($"[Music] ✅ Fade Out เสร็จ");
        Destroy(gameObject);
    }
}

/*
✅ วิธีใช้งาน:

1. สร้าง Empty GameObject ในแต่ละ Scene ชื่อ "Music"
2. แนบ script นี้
3. ใส่เพลงใน Inspector
4. Build!

หากยังไม่มีเสียง ดู Console Log (F8 ใน Development Build)
*/