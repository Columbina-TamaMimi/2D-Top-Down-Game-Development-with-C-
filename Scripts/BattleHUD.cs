using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour
{
	// UI Text สำหรับแสดงชื่อยูนิต
	public Text nameText;

	// UI Text สำหรับแสดงเลเวล
	public Text levelText;

	// Slider สำหรับแสดงค่า HP (หลอดเลือด)
	public Slider hpSlider;

	// ฟังก์ชันใช้ตั้งค่าข้อมูลของยูนิตเริ่มต้นบน HUD
	public void SetHUD(Unit unit)
	{
		nameText.text = unit.unitName; // แสดงชื่อยูนิต
		levelText.text = "Lvl " + unit.unitLevel; // แสดงเลเวล
		hpSlider.maxValue = unit.maxHP; // กำหนดค่ามากสุดของหลอด HP
		hpSlider.value = unit.currentHP; // กำหนดค่าปัจจุบันของ HP
	}

	// ฟังก์ชันนี้ใช้เปลี่ยนค่า HP บน HUD ขณะเล่นเกม
	public void SetHP(int hp)
	{
		hpSlider.value = hp; // ปรับค่าหลอด HP ตามที่ส่งเข้ามา
	}
}
