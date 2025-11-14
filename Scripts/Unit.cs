using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
	public string unitName;
	public int unitLevel;
	public int damage;
	public int maxHP;
	public int currentHP;
	public int defense;

	void Awake()
	{
		currentHP = maxHP;
	}

	// ฟังก์ชันเมื่อได้รับความเสียหาย
	public bool TakeDamage(int dmg)
	{
		dmg -= defense;
		if (dmg < 0) dmg = 0;

		currentHP -= dmg;

		if (currentHP <= 0)
			return true;
		else
			return false;
	}

	// ฟังก์ชันฟื้น HP
	public void Heal(int amount)
	{
		currentHP += amount;
		if (currentHP > maxHP) currentHP = maxHP;
	}

	// คืนค่า HP เป็นเปอร์เซ็นต์
	public float GetHPPercent()
	{
		if (maxHP <= 0) return 0f;
		return (float)currentHP / maxHP;
	}

	// ฟังก์ชันคำนวณดาเมจแบบสุ่ม ±10% + คริติคอล
	public int CalculateDamage(int critChance = 20, float critMultiplier = 2f)
	{
		int dmg = Mathf.RoundToInt(damage * Random.Range(0.9f, 1.1f));
		bool isCrit = Random.Range(0, 100) < critChance;
		if (isCrit) dmg = Mathf.RoundToInt(dmg * critMultiplier);
		return dmg;
	}
}
