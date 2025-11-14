using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public string mapBoundary;
    public List<InventorySaveData> inventorySaveData;
    public List<ChestSaveData> chestSaveData;
    public List<QuestProgressSaveData> questProgressData; // เปลี่ยนเป็น QuestProgressSaveData
}

[System.Serializable]
public class ChestSaveData
{
    public string chestID;
    public bool isOpened;
}

[System.Serializable]
public class QuestProgressSaveData
{
    public string questID;
    public List<QuestObjectiveSaveData> objectives;
}

[System.Serializable]
public class QuestObjectiveSaveData
{
    public ObjectiveType type;
    public string objectiveID;
    public int requiredAmount;
    public int currentAmount;
}