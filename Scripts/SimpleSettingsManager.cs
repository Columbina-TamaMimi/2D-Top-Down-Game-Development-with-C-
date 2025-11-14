using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ระบบตั้งค่าแบบง่ายสุด - Slider เดียวควบคุมเสียงทั้งหมด
/// </summary>
public class SimpleSettingsManager : MonoBehaviour
{
    [Header("Audio Control")]
    public Slider volumeSlider;
    public TextMeshProUGUI volumeText;

    [Header("Audio Source (Optional)")]
    public AudioSource bgmSource; // สำหรับเล่นเพลงตัวอย่าง

    void Start()
    {
        // โหลดค่าที่บันทึกไว้
        LoadVolume();

        // เพิ่ม Listener
        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    void OnVolumeChanged(float value)
    {
        // ตั้งค่าเสียงทั้งหมดในเกม
        AudioListener.volume = value;

        // บันทึกค่า
        PlayerPrefs.SetFloat("Volume", value);
        PlayerPrefs.Save();

        // อัพเดทข้อความ
        UpdateVolumeText(value);

        Debug.Log($"🔊 Volume: {Mathf.RoundToInt(value * 100)}%");
    }

    void UpdateVolumeText(float value)
    {
        if (volumeText != null)
        {
            int percentage = Mathf.RoundToInt(value * 100);
            volumeText.text = $"{percentage}%";
        }
    }

    void LoadVolume()
    {
        // โหลดค่าที่บันทึกไว้ หรือใช้ค่าเริ่มต้น 80%
        float savedVolume = PlayerPrefs.GetFloat("Volume", 0.8f);

        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            OnVolumeChanged(savedVolume);
        }

        Debug.Log($"✅ โหลดระดับเสียง: {Mathf.RoundToInt(savedVolume * 100)}%");
    }

    // ฟังก์ชันรีเซ็ตค่าเริ่มต้น (เรียกจากปุ่ม Reset)
    public void ResetToDefault()
    {
        if (volumeSlider != null)
            volumeSlider.value = 0.8f;

        Debug.Log("🔄 รีเซ็ตระดับเสียงเป็น 80%");
    }

    // ฟังก์ชันเปิด/ปิดเสียง (Toggle Mute)
    public void ToggleMute()
    {
        if (volumeSlider != null)
        {
            // ถ้าเสียงเปิดอยู่ → ปิดเสียง
            if (volumeSlider.value > 0)
            {
                PlayerPrefs.SetFloat("VolumeBeforeMute", volumeSlider.value);
                volumeSlider.value = 0;
                Debug.Log("🔇 ปิดเสียง");
            }
            // ถ้าเสียงปิดอยู่ → เปิดเสียง
            else
            {
                float previousVolume = PlayerPrefs.GetFloat("VolumeBeforeMute", 0.8f);
                volumeSlider.value = previousVolume;
                Debug.Log($"🔊 เปิดเสียง {Mathf.RoundToInt(previousVolume * 100)}%");
            }
        }
    }

    // Static Function สำหรับเรียกใช้จาก Script อื่น
    public static float GetVolume()
    {
        return PlayerPrefs.GetFloat("Volume", 0.8f);
    }

    public static void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("Volume", volume);
        PlayerPrefs.Save();
    }
}