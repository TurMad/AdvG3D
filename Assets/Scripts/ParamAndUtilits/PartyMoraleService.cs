using System.Collections.Generic;
using UnityEngine;

public static class PartyMoraleService
{
    // Возвращает 0..100
    public static int CalculateMoralePercent(List<AdventurerDTO> adventurers)
    {
        if (adventurers == null || adventurers.Count == 0)
            return 0;

        float sum = 0f;
        for (int i = 0; i < adventurers.Count; i++)
            sum += Mathf.Clamp(adventurers[i].energy, 0, 100); // <-- под своё поле

        float avg = sum / adventurers.Count;
        return Mathf.RoundToInt(avg);
    }

    public static float MoraleToMultiplier(int moralePercent)
    {
        moralePercent = Mathf.Clamp(moralePercent, 0, 100);
        return 1f + (moralePercent / 100f); // 75% => 1.75
    }
}