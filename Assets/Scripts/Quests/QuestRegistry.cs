using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestRegistry", menuName = "Game/Quest Registry")]
public class QuestRegistry : ScriptableObject
{
    public List<QuestDefinition> quests = new();
    public QuestDefinition GetById(string id) => quests.Find(q => q != null && q.id == id);
}