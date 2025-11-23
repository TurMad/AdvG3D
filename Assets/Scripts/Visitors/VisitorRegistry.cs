using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VisitorRegistry", menuName = "Game/Visitor Registry")]
public class VisitorRegistry : ScriptableObject
{
    public List<VisitorDefinition> visitors = new();
}