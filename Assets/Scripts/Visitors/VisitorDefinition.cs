using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "Visitor_", menuName = "Game/Visitor Definition")]
public class VisitorDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public int requiredReputation;
    
    [Header("Type")]
    public VisitorKind kind;

    public List<ConditionAsset> conditions = new();
    
    public string[] dialogueTitles;
    
    [Header("Quests")]
    public List<string> questIds = new();
}
