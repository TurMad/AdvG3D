using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public static class VisitorService
{
    private static VisitorRegistry _registry;

    public static void EnsureRegistry() => _registry ??= Resources.Load<VisitorRegistry>("VisitorRegistry");
    
    public static void SyncWithRegistry(GameData data)
    {
        EnsureRegistry();
        if (_registry == null || data == null) return;

        if (data.visitors == null)
            data.visitors = new List<VisitorStateDTO>();

        foreach (var def in _registry.visitors)
        {
            if (def == null || string.IsNullOrEmpty(def.id)) continue;
            if (data.visitors.Any(v => v.id == def.id)) continue;

            data.visitors.Add(new VisitorStateDTO
            {
                id = def.id,
                status = VisitorStatus.Available,
            });
        }
    }
    

    public static bool IsVisitorAvailable(VisitorDefinition v)
    {
        if (v == null) return false;
        var repOk = GameRepository.Data.reputation >= v.requiredReputation;
        var conds = v.conditions;
        var condsOk = (conds == null || conds.Count == 0) ||
                      conds.All(c => GameRepository.Data.completedConditions.Contains(c.conditionId));
        return repOk && condsOk;
    }

    public static List<VisitorDefinition> GetAvailableVisitors()
    {
        EnsureRegistry();
        if (_registry == null) return new();
        return _registry.visitors.Where(IsVisitorAvailable).ToList();
    }

    private static void TryCompleteByEvent(ConditionType type, string targetId)
    {
        EnsureRegistry();
        if (_registry == null) return;

        bool changed = false;

        foreach (var v in _registry.visitors)
        {
            if (v == null || v.conditions == null) continue;

            foreach (var c in v.conditions)
            {
                if (c == null) continue;
                if (GameRepository.Data.completedConditions.Contains(c.conditionId)) continue;

                bool match = c.type == type && c.targetId == targetId;
                if (match)
                {
                    GameRepository.Data.completedConditions.Add(c.conditionId);
                    changed = true;
                }
            }
        }

        if (changed) GameRepository.Save();
    }
}
