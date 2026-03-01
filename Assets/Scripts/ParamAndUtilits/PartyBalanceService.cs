using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PartyBalanceService
{
    private static readonly Dictionary<Rank, int> RankRequirements = new()
    {
        { Rank.G, 3 },
        { Rank.F, 8 },
        { Rank.E, 13 },
        { Rank.D, 18 },
        { Rank.C, 23 },
        { Rank.B, 28 },
        { Rank.A, 33 },
        { Rank.S, 38 },
        { Rank.SS, 43 },
        { Rank.SSS, 48 }
    };

    public static int CalculateBalancePercent(
        List<AdventurerDTO> adventurers,
        Rank rank)
    {
        if (adventurers == null || adventurers.Count == 0)
            return 0;

        int requirement = RankRequirements[rank];

        int attack = adventurers.Sum(a => a.attack);
        int defense = adventurers.Sum(a => a.defense);
        int healing = adventurers.Sum(a => a.healing);
        int buff = adventurers.Sum(a => a.buff);
        int debuff = adventurers.Sum(a => a.debuff);

        float total =
            RoleContribution(attack, requirement) +
            RoleContribution(defense, requirement) +
            RoleContribution(healing,requirement) +
            RoleContribution(buff, requirement) +
            RoleContribution(debuff, requirement);

        return Mathf.RoundToInt(total);
    }
   

    private static float RoleContribution(int value, int required)
    {
        if (required <= 0) return 0f;

        float percent = (float)value / required;
        percent = UnityEngine.Mathf.Clamp01(percent);

        return percent * 20f;
    }
}