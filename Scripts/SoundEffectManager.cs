using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager Instance { get; private set; }

    [Header("Audio Sources")]
    private AudioSource audioSource;
    private AudioSource randomPitchAudioSource;
    private AudioSource voiceAudioSource;

    [Header("Sound Library")]
    [SerializeField] private SoundEffectLibrary soundEffectLibrary;

    [Header("UI")]
    [SerializeField] private Slider sfxSlider;

    [Header("Settings")]
    [SerializeField] private float defaultVolume = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeComponents();
            LoadVolume();
            DontDestroyOnLoad(gameObject);

            // ✅ Debug สำหรับ Build
            Debug.Log("[SFX] SoundEffectManager สร้างสำเร็จ");
        }
        else
        {
            Debug.Log("[SFX] SoundEffectManager ซ้ำ - ทำลาย");
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (Instance != this) return;

        if (sfxSlider != null)
        {
            sfxSlider.value = audioSource != null ? audioSource.volume : defaultVolume;
            sfxSlider.onValueChanged.AddListener(SetVolume);
        }

        // ✅ Preload เสียงเดิน
        PreloadFootstepSounds();
    }

    // ✅ เพิ่มฟังก์ชัน Preload
    private void PreloadFootstepSounds()
    {
        if (soundEffectLibrary != null)
        {
            AudioClip testClip = soundEffectLibrary.GetRandomClipFromGroup("Footstep");
            if (testClip != null)
            {
                testClip.LoadAudioData();
                Debug.Log($"[SFX] ✅ Preload เสียงเดิน: {testClip.name}");
            }
            else
            {
                Debug.LogError("[SFX] ❌ ไม่พบเสียง 'Footstep' ใน Library!");
            }
        }
    }

    private void InitializeComponents()
    {
        AudioSource[] audioSources = GetComponents<AudioSource>();

        // ✅ ถ้าไม่มี AudioSource ให้สร้าง
        if (audioSources.Length < 3)
        {
            Debug.LogWarning($"[SFX] ⚠️ มี AudioSource แค่ {audioSources.Length} ตัว - กำลังสร้างให้ครบ 3");

            while (GetComponents<AudioSource>().Length < 3)
            {
                gameObject.AddComponent<AudioSource>();
            }

            audioSources = GetComponents<AudioSource>();
        }

        if (audioSources.Length >= 3)
        {
            audioSource = audioSources[0];
            randomPitchAudioSource = audioSources[1];
            voiceAudioSource = audioSources[2];

            audioSource.priority = 64;
            audioSource.playOnAwake = false;
            audioSource.loop = false;

            randomPitchAudioSource.priority = 64;
            randomPitchAudioSource.playOnAwake = false;
            randomPitchAudioSource.loop = false;

            voiceAudioSource.priority = 32;
            voiceAudioSource.playOnAwake = false;
            voiceAudioSource.loop = false;

            Debug.Log("[SFX] ✅ AudioSource ทั้ง 3 ตัวพร้อม (Priority: Voice=32, SFX=64)");
        }
        else
        {
            Debug.LogError($"[SFX] ❌ ต้องมี AudioSource 3 ตัว! (มี {audioSources.Length})");
        }

        if (soundEffectLibrary == null)
        {
            Debug.LogError("[SFX] ❌ ไม่พบ SoundEffectLibrary!");
        }
        else
        {
            soundEffectLibrary.Initialize();
            Debug.Log($"[SFX] ✅ โหลด Library สำเร็จ ({soundEffectLibrary.soundEffectGroups.Count} groups)");
        }
    }

    private void LoadVolume()
    {
        float savedVolume = PlayerPrefs.GetFloat("SFXVolume", defaultVolume);
        SetVolume(savedVolume);
        Debug.Log($"[SFX] Volume โหลด: {savedVolume}");
    }

    public void SetVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        if (audioSource != null)
            audioSource.volume = volume;

        if (randomPitchAudioSource != null)
            randomPitchAudioSource.volume = volume;

        if (voiceAudioSource != null)
            voiceAudioSource.volume = volume;

        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }

    public void Play(string soundName, bool randomPitch = false)
    {
        // ✅ เพิ่ม Debug
        Debug.Log($"[SFX] เรียก Play: {soundName}");

        if (soundEffectLibrary == null)
        {
            Debug.LogError("[SFX] ❌ Library เป็น null!");
            return;
        }

        if (string.IsNullOrEmpty(soundName))
        {
            Debug.LogError("[SFX] ❌ soundName เป็นค่าว่าง!");
            return;
        }

        AudioClip audioClip = soundEffectLibrary.GetRandomClipFromGroup(soundName);

        if (audioClip != null)
        {
            Debug.Log($"[SFX] ✅ พบเสียง: {audioClip.name}");

            if (audioSource == null || randomPitchAudioSource == null)
            {
                Debug.LogError("[SFX] ❌ AudioSource เป็น null!");
                return;
            }

            if (randomPitch)
            {
                randomPitchAudioSource.pitch = Random.Range(0.8f, 1.2f);
                randomPitchAudioSource.PlayOneShot(audioClip);
            }
            else
            {
                audioSource.PlayOneShot(audioClip);
            }

            Debug.Log($"[SFX] 🔊 เล่นเสียง: {audioClip.name}");
        }
        else
        {
            Debug.LogError($"[SFX] ❌ ไม่พบเสียง: {soundName}");
        }
    }

    public void PlaySoundFromGroup(string groupName, bool randomPitch = false)
    {
        Play(groupName, randomPitch);
    }

    public void PlayVoice(AudioClip audioClip, float pitch = 1f)
    {
        if (voiceAudioSource != null && audioClip != null)
        {
            voiceAudioSource.pitch = pitch;
            voiceAudioSource.PlayOneShot(audioClip);
        }
    }

    public void PlayClip(AudioClip audioClip, bool randomPitch = false)
    {
        if (audioClip == null)
        {
            Debug.LogWarning("[SFX] ⚠️ AudioClip เป็น null!");
            return;
        }

        if (randomPitch)
        {
            randomPitchAudioSource.pitch = Random.Range(0.8f, 1.2f);
            randomPitchAudioSource.PlayOneShot(audioClip);
        }
        else
        {
            audioSource.PlayOneShot(audioClip);
        }
    }

    public void StopAll()
    {
        if (audioSource != null)
            audioSource.Stop();

        if (randomPitchAudioSource != null)
            randomPitchAudioSource.Stop();

        if (voiceAudioSource != null)
            voiceAudioSource.Stop();
    }

    public void StopVoice()
    {
        if (voiceAudioSource != null)
            voiceAudioSource.Stop();
    }

    public bool IsPlayingVoice()
    {
        return voiceAudioSource != null && voiceAudioSource.isPlaying;
    }

    private void OnDestroy()
    {
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(SetVolume);
        }
    }
}