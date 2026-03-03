using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MissionResolutionService
{
    public static MissionReportDTO ResolveAndWriteReport(QuestStateDTO qs)
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

        var report = data.missionReports.FirstOrDefault(r => r != null && r.questId == qs.id);
        bool isNew = report == null;

        if (isNew)
            report = new MissionReportDTO();
        
        report.reportId = qs.id;
        report.questId = qs.id;
        report.result = result;
        report.expPerAdventurer = expEach;
        report.requiredPower = requiredPower;
        report.partyPowerBase = partyPowerBase;
        report.balanceMultiplier = balanceMultiplier;
        report.partyPowerFinal = partyPowerFinal;

        report.adventurerIds = ids;

        if (isNew)
            data.missionReports.Add(report);
        
        var letter = data.questLetters.FirstOrDefault(l => l != null && l.questId == qs.id);
        bool isNewLetter = letter == null;

        if (isNewLetter)
            letter = new QuestLetterStateDTO();

        letter.questId = qs.id;
        letter.result = result; 
        letter.status = InboxItemStatus.Pending; 
        letter.hoursRemaining = def.notifyHours; 
        
        if (isNewLetter)
            data.questLetters.Add(letter);

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