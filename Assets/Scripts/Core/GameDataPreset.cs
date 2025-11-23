using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DefaultGameData", menuName = "Game/Default Game Data")]
public class GameDataPreset : ScriptableObject
{
    public int gold = 0;
    public int reputation = 0;
    public int guildLevel = 1;
    public int guildExp = 0;
    public List<QuestStateDTO> quests = new();
    public List<AdventurerDTO> adventurers = new();
}