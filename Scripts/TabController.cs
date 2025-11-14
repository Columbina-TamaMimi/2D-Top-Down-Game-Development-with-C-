using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    public Image[] tabImages;
    public GameObject[] pages;

    void Start()
    {
        // ไม่เปิด tab ตอนเริ่ม
    }

    public void ActivateTab(int tabNo)
    {
        // ✅ เช็คว่า Confirmation Panel เปิดอยู่หรือไม่
        if (ExitConfirmationManager.Instance != null &&
            ExitConfirmationManager.Instance.IsPanelOpen())
        {
            Debug.Log("[Tab] ❌ Confirmation Panel เปิดอยู่ ปิด Confirmation ก่อนค่อยเปิด Tab");
            return; // ❌ ห้ามเปิด Tab
        }

        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
            tabImages[i].color = Color.grey;
        }

        pages[tabNo].SetActive(true);
        tabImages[tabNo].color = Color.white;

        Debug.Log($"[Tab] เปิด Tab {tabNo}");
    }

    public bool IsAnyTabOpen()
    {
        foreach (GameObject page in pages)
        {
            if (page.activeSelf)
                return true;
        }
        return false;
    }

    public void CloseAllTabs()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
            tabImages[i].color = Color.grey;
        }
        Debug.Log("[Tab] ปิด Tab ทั้งหมด");
    }
}