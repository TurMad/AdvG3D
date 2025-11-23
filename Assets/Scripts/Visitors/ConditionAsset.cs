using UnityEngine;

[CreateAssetMenu(fileName="Cond_", menuName="Game/Condition")]
public class ConditionAsset : ScriptableObject
{
    public string conditionId;   
    public ConditionType type;   
    public string targetId;      
}