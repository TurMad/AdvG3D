using System.Linq;
using UnityEngine;

public static class AdventurerService
{
    private static AdventurerRegistry _registry;

    public static void EnsureRegistry()
    {
        if (_registry == null)
            _registry = Resources.Load<AdventurerRegistry>("AdventurerRegistry");
    }

    public static AdventurerDefinition GetDefinition(string id)
    {
        EnsureRegistry();
        return _registry?.GetById(id);
    }

    public static AdventurerDTO GetState(GameData data, string id)
    {
        return data.adventurers.Find(a => a.id == id);
    }
    
    public static int GetEnergy(string adventurerId)
    {
        var data = GameRepository.Data;
        if (data == null || data.adventurers == null)
            return 0;

        var adv = data.adventurers.FirstOrDefault(a => a != null && a.id == adventurerId);
        return adv != null ? adv.energy : 0;
    }

    public static void SyncWithRegistry(GameData data)
    {
        EnsureRegistry();
        if (_registry == null) return;

        foreach (var def in _registry.adventurers)
        {
            if (def == null) continue;

            var state = data.adventurers.Find(a => a.id == def.id);
            if (state == null)
            {
                data.adventurers.Add(new AdventurerDTO
                {
                    id = def.id,
                    level = def.startLevel,
                    attack = def.baseAttack,
                    defense = def.baseDefense,
                    buff = def.baseBuff,
                    debuff = def.baseDebuff,
                    healing = def.baseHealing,
                    status = AdventurerStatus.NotReceived
                });
            }
            foreach (var adv in data.adventurers)
            {
                if (adv == null) continue;

                if (adv.level < 1) adv.level = 1;
                if (adv.xpToNext <= 0)
                    ExperienceService.RefreshXpToNext(adv);
            }
        }
    }
}