using System.Collections.Generic;
using UnityEngine;

public static class AdventurerLevelUpService
{
    public static void ApplyLevelUp(AdventurerDTO adv)
    {
        if (adv == null) return;

        var def = AdventurerService.GetDefinition(adv.id);
        if (def == null || def.growth == null || def.growth.Count == 0)
        {
            // fallback: если не настроено — качаем Attack по умолчанию
            adv.attack += 1;
            return;
        }

        var chosen = ChooseWeighted(def.growth);
        if (chosen == null) return;

        AddStat(adv, chosen.stat, chosen.amount);
    }

    private static StatGrowthWeight ChooseWeighted(List<StatGrowthWeight> list)
    {
        int sum = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null) continue;
            sum += Mathf.Max(1, list[i].weight);
        }

        if (sum <= 0) return null;

        int roll = Random.Range(0, sum);
        int acc = 0;

        for (int i = 0; i < list.Count; i++)
        {
            var it = list[i];
            if (it == null) continue;

            acc += Mathf.Max(1, it.weight);
            if (roll < acc)
                return it;
        }

        return list[0];
    }

    private static void AddStat(AdventurerDTO adv, AdventurerStatType stat, int amount)
    {
        if (amount < 1) amount = 1;

        switch (stat)
        {
            case AdventurerStatType.Attack:   adv.attack += amount;   break;
            case AdventurerStatType.Defense:  adv.defense += amount;  break;
            case AdventurerStatType.Buff:     adv.buff += amount;     break;
            case AdventurerStatType.Debuff:   adv.debuff += amount;   break;
            case AdventurerStatType.Healing:  adv.healing += amount;  break;
        }
    }
}