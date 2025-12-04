using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "Visitor_", menuName = "Game/Visitor Definition")]
public class VisitorDefinition : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string displayName;
    public Sprite portrait;
    
    [Header("Type")]
    public VisitorKind kind;
    
    [Header("Requirements")]
    public int requiredReputation;
    public List<ConditionAsset> conditions = new();
    
    public string[] dialogueTitles;
    
    [Header("Quests")]
    public List<string> questIds = new();
}
