using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// คลังเก็บเสียง Sound Effect ทั้งหมด
/// ใช้ ScriptableObject เพื่อสร้างเป็น Asset
/// </summary>
[CreateAssetMenu(fileName = "SoundEffectLibrary", menuName = "Audio/Sound Effect Library")]
public class SoundEffectLibrary : ScriptableObject
{
    [Header("Sound Effect Groups")]
    public List<SoundEffectGroup> soundEffectGroups = new List<SoundEffectGroup>();

    // Dictionary สำหรับค้นหาเสียงเร็วขึ้น
    private Dictionary<string, SoundEffectGroup> groupDictionary;

    /// <summary>
    /// สร้าง Dictionary เมื่อต้องการใช้
    /// </summary>
    public void Initialize()
    {
        groupDictionary = new Dictionary<string, SoundEffectGroup>();

        foreach (var group in soundEffectGroups)
        {
            if (!groupDictionary.ContainsKey(group.name))
            {
                groupDictionary.Add(group.name, group);
            }
            else
            {
                Debug.LogWarning($"⚠️ Sound Group ชื่อ '{group.name}' ซ้ำ!");
            }
        }
    }

    /// <summary>
    /// ดึง Sound Group ตามชื่อ
    /// </summary>
    public SoundEffectGroup GetSoundGroup(string groupName)
    {
        if (groupDictionary == null || groupDictionary.Count == 0)
            Initialize();

        if (groupDictionary.TryGetValue(groupName, out SoundEffectGroup group))
        {
            return group;
        }

        Debug.LogWarning($"⚠️ ไม่พบ Sound Group '{groupName}' ใน Library!");
        return null;
    }

    /// <summary>
    /// เล่นเสียงสุ่มจาก Group
    /// </summary>
    public AudioClip GetRandomClipFromGroup(string groupName)
    {
        SoundEffectGroup group = GetSoundGroup(groupName);

        if (group != null && group.audioClips.Count > 0)
        {
            return group.audioClips[Random.Range(0, group.audioClips.Count)];
        }

        return null;
    }
}

// ========================================
// Sound Effect Group (แบบในรูป)
// ========================================

[System.Serializable]
public class SoundEffectGroup
{
    [Header("Group Info")]
    public string name = "New Group"; // ชื่อ Group (Chest, Footstep, etc.)

    [Header("Audio Clips")]
    public List<AudioClip> audioClips = new List<AudioClip>(); // รายการเสียง

    [Header("Settings")]
    [Range(0f, 1f)]
    public float volume = 1f;

    [Range(0.1f, 3f)]
    public float pitch = 1f;

    public bool loop = false; // เล่นซ้ำหรือไม่

    public bool randomizeVolume = false;
    [Range(0f, 0.5f)]
    public float volumeVariation = 0.1f;

    public bool randomizePitch = false;
    [Range(0f, 0.5f)]
    public float pitchVariation = 0.1f;

    /// <summary>
    /// ดึงเสียงสุ่มจาก Group
    /// </summary>
    public AudioClip GetRandomClip()
    {
        if (audioClips.Count > 0)
            return audioClips[Random.Range(0, audioClips.Count)];
        return null;
    }

    public float GetRandomVolume()
    {
        if (randomizeVolume)
            return volume + Random.Range(-volumeVariation, volumeVariation);
        return volume;
    }

    public float GetRandomPitch()
    {
        if (randomizePitch)
            return pitch + Random.Range(-pitchVariation, pitchVariation);
        return pitch;
    }
}

// ========================================
// Sound Categories (ประเภทเสียง)
// ========================================

public enum SoundCategory
{
    UI,         // เสียง UI (กดปุ่ม, เปิดเมนู)
    Player,     // เสียงผู้เล่น (เดิน, กระโดด, โจมตี)
    Enemy,      // เสียงศัตรู
    Environment,// เสียงสภาพแวดล้อม (ลม, น้ำ)
    Item,       // เสียงไอเทม (เก็บ, ใช้)
    Weapon,     // เสียงอาวุธ
    Magic,      // เสียงเวทย์
    Ambience,   // เสียงบรรยากาศพื้นหลัง
    Other       // อื่นๆ
}