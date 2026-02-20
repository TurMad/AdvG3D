using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Quest_", menuName = "Game/Quest Definition")]
public class QuestDefinition : ScriptableObject
{
    [Header("Identity")]
    public string id;            
    public string title;
    
    [Header("Visual")]
    public Sprite icon;
    
    [Header("Times")]
    public int travelHours = 1;     
    public int executeHours = 1;  
    public int notifyHours = 1;  
    
    [Header("Requirements")]
    public int requiredReputation = 0; 
    public int requiredPower = 0;

    [Header("Rewards")]
    public int baseGold = 0;       
    
    public string dialogueTitles;
    
    public List<ConditionAsset> unlockConditions = new();
}

