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
}

[Serializable]
public class AdventurerDTO
{
    public string adventurerId; // ваш ID из базы
    public int level;
    public int exp;
    public string state;        // "Available/Busy"
}