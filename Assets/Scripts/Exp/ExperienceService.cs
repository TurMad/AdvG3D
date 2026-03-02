using UnityEngine;

public static class ExperienceService
{
    private static ExperienceTable _table;

    private static void Ensure()
    {
        if (_table == null)
            _table = Resources.Load<ExperienceTable>("ExperienceTable");
    }

    public static int GetXpToNext(int level)
    {
        Ensure();
        return _table != null ? _table.GetXpToNext(level) : 100;
    }

    public static void RefreshXpToNext(AdventurerDTO adv)
    {
        if (adv == null) return;
        adv.xpToNext = GetXpToNext(adv.level);
    }

    public static void AddXp(AdventurerDTO adv, int amount)
    {
        if (adv == null || amount <= 0) return;

        if (adv.xpToNext <= 0)
            RefreshXpToNext(adv);

        adv.currentXp += amount;

        while (adv.currentXp >= adv.xpToNext && adv.xpToNext > 0)
        {
            adv.currentXp -= adv.xpToNext;
            adv.level++;
            AdventurerLevelUpService.ApplyLevelUp(adv);
            RefreshXpToNext(adv);
        }
    }
}