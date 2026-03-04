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
    
    public List<MissionReportDTO> missionReports = new();
    
    public List<QuestLetterStateDTO> questLetters = new();
}

[Serializable]
public class  QuestStateDTO
{
    public string id;
    public int tradedGold;   
    public Rank questRank = Rank.G;
    
    public int guildGold;       
    public int adventurersGold;
    
    public QuestStatus status; 
    public float travelElapsedSeconds;
    public int executeHoursRemaining;
    public int deadlineHoursRemaining;
    
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
    
    public int level;
    
    public int currentXp;
    public int xpToNext;
    
    public int energy = 100;

    public int attack;
    public int defense;
    public int buff;
    public int debuff;
    public int healing;
    
    public AdventurerStatus status;
}

[Serializable]
public class VisitorStateDTO
{
    public string id;            
    public VisitorStatus status; 
    public string queuedQuestId;
}

[Serializable]
public class MissionReportDTO
{
    public string reportId;                
    public string questId;

    public MissionResult result;
    public InboxItemStatus status;

    public int expPerAdventurer;
    
    public int requiredPower;
    
    public int partyPowerBase;
    public float balanceMultiplier;
    public float moraleMultiplier;
    public int partyPowerFinal;

    public List<string> adventurerIds = new();
}

[Serializable]
public class QuestLetterStateDTO
{
    public string questId;
    public MissionResult result;
    public InboxItemStatus status;
    public int hoursRemaining;  
}

public enum MissionResult
{
    None = 0,
    Success = 1,
    Fail = 2
}

public enum InboxItemStatus
{
    None = 0,
    Pending = 1,   
    OnDesk = 2,    
    Read = 3      
}

