using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MissionResolutionService
{
    public static MissionReportDTO ResolveAndWriteReport(QuestStateDTO qs, int nextIndex)
    {
        var data = GameRepository.Data;
        if (data == null) return null;

        var def = QuestService.GetDef(qs.id);
        if (def == null) return null;

        var ids = CollectAssignedIds(qs);

        int partyPowerBase = 0;
        var selectedAdventurers = new List<AdventurerDTO>(ids.Count);

        if (data.adventurers != null && ids.Count > 0)
        {
            foreach (var id in ids)
            {
                var adv = data.adventurers.FirstOrDefault(a => a != null && a.id == id);
                if (adv == null) continue;

                selectedAdventurers.Add(adv);
                partyPowerBase += CombatPowerCalculator.GetVisiblePower(adv);
            }
        }

        int requiredPower = def.requiredPower;

        // ===== Balance multiplier =====
        // ВАЖНО: замени qs.questRank на реальное поле ранга в твоём QuestStateDTO
        int balancePercent = 0;
        if (selectedAdventurers.Count > 0)
            balancePercent = PartyBalanceService.CalculateBalancePercent(selectedAdventurers, qs.questRank);

        float balanceMultiplier = 1f + Mathf.Clamp01(balancePercent / 100f);
        
        int moralePercent = PartyMoraleService.CalculateMoralePercent(selectedAdventurers);
        float moraleMultiplier = PartyMoraleService.MoraleToMultiplier(moralePercent);

        int partyPowerFinal = Mathf.RoundToInt(partyPowerBase * balanceMultiplier);

        var result = partyPowerFinal >= requiredPower ? MissionResult.Success : MissionResult.Fail;

        // ===== EXP (только при успехе) =====
        int expEach = 0;
        if (result == MissionResult.Success && ids.Count > 0)
        {
            expEach = def.baseExp / ids.Count;

            foreach (var adv in selectedAdventurers)
                ExperienceService.AddXp(adv, expEach);
        }

        if (data.missionReports == null)
            data.missionReports = new List<MissionReportDTO>();

        var report = new MissionReportDTO
        {
            reportId = $"{qs.id}_{nextIndex}",
            questId = qs.id,
            result = result,
            expPerAdventurer = expEach,
            requiredPower = requiredPower,
            partyPowerBase = partyPowerBase,
            partyPowerFinal = partyPowerFinal,
            balanceMultiplier = balanceMultiplier, 
            moraleMultiplier = moraleMultiplier,
            adventurerIds = ids
        };

        data.missionReports.Add(report);
        GameRepository.Save();
        return report;
    }

    private static List<string> CollectAssignedIds(QuestStateDTO qs)
    {
        var ids = new List<string>(5);
        if (!string.IsNullOrEmpty(qs.assignedAdventurer1)) ids.Add(qs.assignedAdventurer1);
        if (!string.IsNullOrEmpty(qs.assignedAdventurer2)) ids.Add(qs.assignedAdventurer2);
        if (!string.IsNullOrEmpty(qs.assignedAdventurer3)) ids.Add(qs.assignedAdventurer3);
        if (!string.IsNullOrEmpty(qs.assignedAdventurer4)) ids.Add(qs.assignedAdventurer4);
        if (!string.IsNullOrEmpty(qs.assignedAdventurer5)) ids.Add(qs.assignedAdventurer5);
        return ids;
    }
}