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
                    attack = def.baseAttack,
                    defense = def.baseDefense,
                    buff = def.baseBuff,
                    debuff = def.baseDebuff,
                    healing = def.baseHealing,
                    status = AdventurerStatus.NotReceived
                });
            }
        }
    }
}