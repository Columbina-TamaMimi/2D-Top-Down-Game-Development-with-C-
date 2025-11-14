using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType { AGGRESSIVE, DEFENSIVE, BALANCED, BERSERKER, SMART }

public class EnemyAI : MonoBehaviour
{
    [Header("ประเภทมอน")]
    public EnemyType enemyType = EnemyType.BALANCED;

    [Header("โอกาสทำแต่ละ Action (%)")]
    [Range(0, 100)] public int attackChance = 70;
    [Range(0, 100)] public int heavyAttackChance = 20;
    [Range(0, 100)] public int healChance = 10;
    [Range(0, 100)] public int defendChance = 10;
    [Range(0, 100)] public int specialSkillChance = 5;

    [Header("ค่าความเสียหาย/ฮีล/ป้องกัน")]
    public float heavyAttackMultiplier = 1.5f;
    public int healAmount = 15;
    public int defendBonus = 5; // ✅ ลดจาก 20 เป็น 5
    [Tooltip("ป้องกันได้กี่เทิร์น (0 = 1 เทิร์น)")]
    public int defendDuration = 0; // ✅ ป้องกันได้แค่ 1 เทิร์น

    [Header("Skill พิเศษ")]
    public bool hasSpecialSkill = false;
    public string specialSkillName = "Rage";

    private Unit unit;
    private bool isDefending = false;
    private int originalDefense;
    private int defendTurnsLeft = 0; // ✅ นับเทิร์นที่ป้องกันเหลือ

    void Awake()
    {
        unit = GetComponent<Unit>();
        if (unit != null) originalDefense = unit.defense;
    }

    public EnemyAction DecideAction(Unit playerUnit)
    {
        // ✅ ลดป้องกันทุกเทิร์น
        if (isDefending)
        {
            defendTurnsLeft--;
            if (defendTurnsLeft <= 0)
            {
                ClearDefense();
            }
        }

        AdjustBehaviorByType();
        EnemyAction action = new EnemyAction();
        float hpPercent = unit.GetHPPercent();

        // SMART AI
        if (enemyType == EnemyType.SMART)
        {
            action = SmartDecision(playerUnit, hpPercent);
        }
        else if (hpPercent < 25f && Random.Range(0, 100) < healChance)
        {
            action.actionType = ActionType.HEAL;
            action.value = healAmount;
            action.message = unit.unitName + " รักษาตัวเอง!";
        }
        // ✅ ป้องกันได้แค่ตอน HP ต่ำ และไม่ได้ป้องกันอยู่แล้ว
        else if (!isDefending && hpPercent < 40f && Random.Range(0, 100) < defendChance)
        {
            action.actionType = ActionType.DEFEND;
            action.value = defendBonus;
            action.message = unit.unitName + " ป้องกันตัวเอง!";
        }
        else if (hasSpecialSkill && Random.Range(0, 100) < specialSkillChance)
        {
            action = SpecialSkill(playerUnit);
        }
        else if (Random.Range(0, 100) < heavyAttackChance)
        {
            action.actionType = ActionType.HEAVY_ATTACK;
            action.value = unit.damage;
            action.message = unit.unitName + " ใช้โจมตีหนัก!";
        }
        else
        {
            action.actionType = ActionType.ATTACK;
            action.value = unit.damage;
            action.message = unit.unitName + " โจมตี!";
        }

        return action;
    }

    void AdjustBehaviorByType()
    {
        switch (enemyType)
        {
            case EnemyType.AGGRESSIVE:
                attackChance = 85;
                heavyAttackChance = 30;
                healChance = 5;
                defendChance = 3;  // ✅ ลดลง
                break;

            case EnemyType.DEFENSIVE:
                attackChance = 60;     // ✅ เพิ่มให้โจมตีบ้าง
                heavyAttackChance = 10;
                healChance = 20;       // ✅ ลดลง
                defendChance = 15;     // ✅ ลดลง
                break;

            case EnemyType.BALANCED:
                attackChance = 70;
                heavyAttackChance = 20;
                healChance = 15;
                defendChance = 10;     // ✅ ลดลงนิดหน่อย
                break;

            case EnemyType.BERSERKER:
                if (unit.GetHPPercent() < 30f)
                {
                    attackChance = 95;
                    heavyAttackChance = 60;  // ✅ เพิ่มให้ดุขึ้น
                    healChance = 0;
                    defendChance = 0;
                }
                else
                {
                    // ✅ เพิ่มเคส HP สูง - โจมตีบ่อยกว่า Balanced
                    attackChance = 80;
                    heavyAttackChance = 30;
                    healChance = 5;
                    defendChance = 3;
                }
                break;

            case EnemyType.SMART:
                // ใช้ SmartDecision() จัดการเอง
                // ไม่ต้องปรับค่าที่นี่
                break;
        }
    }

    EnemyAction SmartDecision(Unit playerUnit, float hpPercent)
    {
        EnemyAction action = new EnemyAction();
        float playerHPPercent = playerUnit.GetHPPercent();

        if (playerHPPercent < 20f)
        {
            // ผู้เล่น HP ต่ำ → เข้าตัดสินให้ตาย
            action.actionType = ActionType.HEAVY_ATTACK;
            action.value = unit.damage;
            action.message = unit.unitName + " เห็นจุดอ่อนของคุณ! โจมตีแรง!";
        }
        else if (hpPercent < 40f && playerHPPercent > 50f && !isDefending) // ✅ เพิ่ม !isDefending
        {
            // ตัวเอง HP ต่ำ แต่ผู้เล่น HP สูง → ฮีลหรือป้องกัน
            if (Random.Range(0, 2) == 0)
            {
                action.actionType = ActionType.HEAL;
                action.value = healAmount;
                action.message = unit.unitName + " ฟื้นตัว!";
            }
            else
            {
                action.actionType = ActionType.DEFEND;
                action.value = defendBonus;
                action.message = unit.unitName + " ป้องกันตัว!";
            }
        }
        else
        {
            // สถานการณ์ปกติ → โจมตี
            action.actionType = ActionType.ATTACK;
            action.value = unit.damage;
            action.message = unit.unitName + " โจมตี!";
        }

        return action;
    }

    EnemyAction SpecialSkill(Unit playerUnit)
    {
        EnemyAction action = new EnemyAction();
        switch (specialSkillName)
        {
            case "Rage":
                action.actionType = ActionType.SPECIAL;
                action.value = unit.damage;
                action.message = unit.unitName + " ใช้ " + specialSkillName + "!";
                break;
            default:
                action.actionType = ActionType.ATTACK;
                action.value = unit.damage;
                action.message = unit.unitName + " โจมตี!";
                break;
        }
        return action;
    }

    public void ClearDefense()
    {
        if (isDefending && unit != null)
        {
            unit.defense = originalDefense;
            isDefending = false;
            defendTurnsLeft = 0;
            Debug.Log($"🛡️ {unit.unitName} หมดเวลาป้องกัน! Defense: {unit.defense}");
        }
    }

    public void ApplyDefense(int bonus)
    {
        if (unit != null)
        {
            unit.defense += bonus;
            isDefending = true;
            defendTurnsLeft = defendDuration + 1; // ตั้งเวลาป้องกัน
            Debug.Log($"🛡️ {unit.unitName} ป้องกัน! Defense: {unit.defense} เป็นเวลา {defendTurnsLeft} เทิร์น");
        }
    }
}

[System.Serializable]
public class EnemyAction
{
    public ActionType actionType;
    public int value;
    public string message;
}

public enum ActionType { ATTACK, HEAVY_ATTACK, HEAL, DEFEND, SPECIAL }