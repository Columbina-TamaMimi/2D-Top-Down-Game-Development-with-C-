using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST }

public class BattleSystem : MonoBehaviour
{
	[Header("Prefab & ตำแหน่ง")]
	public GameObject playerPrefab;
	public GameObject enemyPrefab;
	public Transform playerBattleStation;
	public Transform enemyBattleStation;

	private Unit playerUnit;
	private Unit enemyUnit;

	[Header("UI")]
	public Text dialogueText;
	public BattleHUD playerHUD;
	public BattleHUD enemyHUD;
	public Button returnButton;
	[Range(0, 100)] public int critChance = 20;

	public BattleState state;

	void Start()
	{
		state = BattleState.START;
		if (returnButton != null) returnButton.gameObject.SetActive(false);
		StartCoroutine(SetupBattle());
	}

	IEnumerator SetupBattle()
	{
		GameObject playerGO = Instantiate(playerPrefab, playerBattleStation);
		playerUnit = playerGO.GetComponent<Unit>();

		GameObject enemyGO = Instantiate(enemyPrefab, enemyBattleStation);
		enemyUnit = enemyGO.GetComponent<Unit>();

		dialogueText.text = "ศัตรู " + enemyUnit.unitName + " ปรากฏตัว!";
		playerHUD.SetHUD(playerUnit);
		enemyHUD.SetHUD(enemyUnit);

		yield return new WaitForSeconds(2f);
		state = BattleState.PLAYERTURN;
		dialogueText.text = "ถึงตาคุณแล้ว!";
	}

	IEnumerator PlayerAttack()
	{
		state = BattleState.ENEMYTURN;
		int dmg = playerUnit.CalculateDamage(critChance);
		bool isCrit = dmg > playerUnit.damage;
		dialogueText.text = isCrit ? "🔥 คริ! คุณโจมตีแรง!" : "คุณโจมตีศัตรู!";
		yield return new WaitForSeconds(1f);

		bool isDead = enemyUnit.TakeDamage(dmg);
		enemyHUD.SetHP(enemyUnit.currentHP);
		yield return new WaitForSeconds(1f);

		if (isDead)
		{
			state = BattleState.WON;
			EndBattle();
		}
		else
		{
			StartCoroutine(EnemyTurn());
		}
	}

	IEnumerator PlayerHeal()
	{
		state = BattleState.ENEMYTURN;
		int healAmount = Mathf.RoundToInt(Random.Range(10f, 20f));
		playerUnit.Heal(healAmount);
		playerHUD.SetHP(playerUnit.currentHP);
		dialogueText.text = "คุณฟื้น HP " + healAmount + " หน่วย!";
		yield return new WaitForSeconds(1.5f);
		StartCoroutine(EnemyTurn());
	}

	IEnumerator EnemyTurn()
	{
		dialogueText.text = enemyUnit.unitName + " กำลังตัดสินใจ...";
		yield return new WaitForSeconds(1f);

		EnemyAI ai = enemyUnit.GetComponent<EnemyAI>();
		if (ai == null)
		{
			Debug.LogWarning("ไม่มี EnemyAI");
			yield break;
		}

		EnemyAction action = ai.DecideAction(playerUnit);
		dialogueText.text = action.message;
		yield return new WaitForSeconds(1f);

		switch (action.actionType)
		{
			case ActionType.ATTACK:
			case ActionType.HEAVY_ATTACK:
			case ActionType.SPECIAL:
				float multiplier = 1f;
				if (action.actionType == ActionType.HEAVY_ATTACK) multiplier = ai.heavyAttackMultiplier;
				if (action.actionType == ActionType.SPECIAL) multiplier = 2f;

				int dmg = Mathf.RoundToInt(enemyUnit.damage * multiplier * Random.Range(0.9f, 1.1f));
				int enemyCritChance = 20;
				bool isCrit = Random.Range(0, 100) < enemyCritChance;
				if (isCrit) dmg = Mathf.RoundToInt(dmg * 2f);
				if (isCrit) dialogueText.text += " 🔥 คริ!";

				bool isDead = playerUnit.TakeDamage(dmg);
				playerHUD.SetHP(playerUnit.currentHP);
				yield return new WaitForSeconds(1f);

				if (isDead)
				{
					state = BattleState.LOST;
					EndBattle();
					yield break;
				}
				break;

			case ActionType.HEAL:
				enemyUnit.Heal(action.value);
				enemyHUD.SetHP(enemyUnit.currentHP);
				break;

			case ActionType.DEFEND:
				ai.ApplyDefense(action.value);
				Debug.Log($"🛡️ {enemyUnit.unitName} ป้องกัน! Defense: {enemyUnit.defense}");
				break;
		}

		yield return new WaitForSeconds(1f);
		state = BattleState.PLAYERTURN;
		dialogueText.text = "ถึงตาคุณแล้ว!";
	}

	public void OnAttackButton()
	{
		if (state != BattleState.PLAYERTURN) return;
		StartCoroutine(PlayerAttack());
	}

	public void OnHealButton()
	{
		if (state != BattleState.PLAYERTURN) return;
		StartCoroutine(PlayerHeal());
	}

	// 🔥 แก้ไขฟังก์ชันนี้
	void EndBattle()
	{
		dialogueText.text = state == BattleState.WON ? "🎉 คุณชนะ!" : "💀 คุณแพ้...";

		// 🔥 เรียก BattleManager แทน
		BattleManager battleManager = BattleManager.Instance;
		if (battleManager != null)
		{
			if (state == BattleState.WON)
			{
				battleManager.WinBattle();
			}
			else if (state == BattleState.LOST)
			{
				battleManager.LoseBattle();
			}
		}
		else
		{
			Debug.LogError("❌ ไม่พบ BattleManager!");
			// Fallback เดิม
			if (returnButton != null)
			{
				returnButton.gameObject.SetActive(true);
				returnButton.onClick.RemoveAllListeners();
				returnButton.onClick.AddListener(ReturnToPreviousScene);
			}
			else
			{
				StartCoroutine(AutoReturn());
			}
		}
	}

	// 🔥 เก็บไว้เป็น Fallback (กรณีไม่มี BattleManager)
	IEnumerator AutoReturn()
	{
		yield return new WaitForSeconds(3f);
		ReturnToPreviousScene();
	}

	// 🔥 เก็บไว้เป็น Fallback (กรณีไม่มี BattleManager)
	public void ReturnToPreviousScene()
	{
		SceneManager.LoadScene(PlayerPrefs.GetString("CurrentScene", "SampleScene"));
	}
}