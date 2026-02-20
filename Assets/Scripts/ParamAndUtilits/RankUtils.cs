using UnityEngine;

public static class RankUtils
{
    public static Rank GetRankByLevel(int level)
    {
        level = Mathf.Max(1, level);

        int index = (level - 1) / 5; // 0..∞
        index = Mathf.Clamp(index, 0, (int)Rank.SSS);

        return (Rank)index;
    }
}