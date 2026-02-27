using System.Collections.Generic;
using System.Linq;

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
        if (data.adventurers != null && ids.Count > 0)
        {
            foreach (var id in ids)
            {
                var adv = data.adventurers.FirstOrDefault(a => a != null && a.id == id);
                if (adv == null) continue;

                partyPowerBase += CombatPowerCalculator.GetVisiblePower(adv);
            }
        }

        int requiredPower = def.requiredPower;
        int partyPowerFinal = partyPowerBase;

        var result = partyPowerFinal >= requiredPower ? MissionResult.Success : MissionResult.Fail;

        int expEach = 0;
        if (result == MissionResult.Success && ids.Count > 0)
        {
            expEach = def.baseExp / ids.Count;

            foreach (var id in ids)
            {
                var adv = data.adventurers.FirstOrDefault(a => a != null && a.id == id);
                if (adv == null) continue;

                ExperienceService.AddXp(adv, expEach);
            }
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