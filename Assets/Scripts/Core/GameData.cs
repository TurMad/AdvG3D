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
    
    public List<VisitorStateDTO> visitors = new();
}

[Serializable]
public class  QuestStateDTO
{
    public string id;
    public int tradedGold;    
    public QuestStatus status; 
    public float travelElapsedSeconds;
    public int executeHoursRemaining;
    
    public string assignedAdventurer1;
    public string assignedAdventurer2;
    public string assignedAdventurer3;
    public string assignedAdventurer4;
    public string assignedAdventurer5;
}

[Serializable]
public class AdventurerDTO
{
    public string id;

    public int attack;
    public int defense;
    public int buff;
    public int debuff;
    public int healing;
    
    public AdventurerStatus status;
}

[System.Serializable]
public class VisitorStateDTO
{
    public string id;            
    public VisitorStatus status; 
    public string queuedQuestId;
}