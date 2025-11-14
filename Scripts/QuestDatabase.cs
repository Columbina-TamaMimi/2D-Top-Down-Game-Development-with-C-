using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestDatabase", menuName = "Quest/Quest Database")]
public class QuestDatabase : ScriptableObject
{
    public List<Quest> allQuests = new List<Quest>();

    public Quest GetQuestByID(string questID)
    {
        return allQuests.Find(q => q.questID == questID);
    }
}