using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public int gold = 0;
    public int reputation = 0;
    public int guildLevel = 1;
    public int guildExp = 0;
    public int day = 1;
    public int hour = 8;
    
    public int maxActiveQuests = 5;    
    public int currentActiveQuests = 0;

    public HashSet<string> completedConditions = new();
    
    public List<QuestStateDTO> quests = new();

    public List<AdventurerDTO> adventurers = new();
}

[Serializable]
public class  QuestStateDTO
{
    public string id;
    public int tradedGold;    
    public QuestStatus status; 
    public float travelElapsedSeconds;
    public int executeHoursRemaining;
}

[Serializable]
public class AdventurerDTO
{
    public string adventurerId; 
    public int level;
    public int exp;
    public string state;
}

[System.Serializable]
public class VisitorStateDTO
{
    public string id;            
    public VisitorStatus status; 
    public int dayAdded;         
}