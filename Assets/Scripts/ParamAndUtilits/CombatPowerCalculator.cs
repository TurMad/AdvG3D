using UnityEngine;

public static class CombatPowerCalculator
{
    public static int GetBasePower(AdventurerDTO a)
    {
        if (a == null) return 0;

        return Mathf.Max(0,
            a.attack +
            a.defense +
            a.healing +
            a.buff +
            a.debuff
        );
    }

    public static int GetVisiblePower(AdventurerDTO a)
    {
        int basePower = GetBasePower(a);

        // TODO позже:
        // basePower += perksFlat;
        // basePower = Mathf.RoundToInt(basePower * perksPercent);
        // basePower += equipmentFlat;
        // ...

        return basePower;
    }
}