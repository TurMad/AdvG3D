using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AdventurerRegistry", menuName = "Game/Adventurer Registry")]
public class AdventurerRegistry : ScriptableObject
{
    public List<AdventurerDefinition> adventurers = new();

    public AdventurerDefinition GetById(string id)
    {
        return adventurers.Find(a => a != null && a.id == id);
    }
}