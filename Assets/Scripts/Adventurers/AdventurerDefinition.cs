using UnityEngine;

[CreateAssetMenu(fileName = "Adv_", menuName = "Game/Adventurer Definition")]
public class AdventurerDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    
    [Header("Base Stats")]
    public int baseAttack;
    public int baseDefense;
    public int baseBuff;
    public int baseDebuff;
    public int baseHealing;
}