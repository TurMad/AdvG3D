using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "Adv_", menuName = "Game/Adventurer Definition")]
public class AdventurerDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    
    public Sprite portrait;
    
    public int startLevel = 1;
    
    [Header("Attack Types")]
    public List<AttackType> attackTypes = new();
    
    [Header("Base Stats")]
    public int baseAttack;
    public int baseDefense;
    public int baseBuff;
    public int baseDebuff;
    public int baseHealing;
}