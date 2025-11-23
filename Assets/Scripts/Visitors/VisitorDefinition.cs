using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "Visitor_", menuName = "Game/Visitor Definition")]
public class VisitorDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public int requiredReputation;
    public List<string> conversations = new();

    public List<ConditionAsset> conditions = new();
    
    public string[] dialogueTitles;
}