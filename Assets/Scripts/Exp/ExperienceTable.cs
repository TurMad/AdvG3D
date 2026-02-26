using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ExperienceTable", menuName = "Game/Experience Table")]
public class ExperienceTable : ScriptableObject
{
    // index 0: с 1 на 2, index 1: с 2 на 3 ...
    public List<int> xpToNextLevel = new();

    public int GetXpToNext(int level)
    {
        if (level < 1) level = 1;

        if (xpToNextLevel == null || xpToNextLevel.Count == 0)
            return 100;

        int index = level - 1;
        if (index < 0) index = 0;
        if (index >= xpToNextLevel.Count)
            return xpToNextLevel[^1]; // если уровни выше списка — берем последнее значение

        return xpToNextLevel[index];
    }
}