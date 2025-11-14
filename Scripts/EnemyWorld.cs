using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyWorld : MonoBehaviour
{
    [Header("Battle Settings")]
    public string battleSceneName = "BattleScene";

    public void RemoveAfterBattle()
    {
        PlayerPrefs.SetInt(gameObject.name + "_defeated", 1);
        PlayerPrefs.Save();
        Destroy(gameObject);
    }
}
