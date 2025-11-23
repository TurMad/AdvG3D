using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public static class QuestService
{
    private static QuestRegistry _registry;
    private static bool _subscribed;

    // ---------- Registry ----------
    public static void EnsureRegistry() => _registry ??= Resources.Load<QuestRegistry>("QuestRegistry");
    public static QuestDefinition GetDef(string id) { EnsureRegistry(); return _registry?.GetById(id); }

    // ---------- Init/Sync with save ----------
    public static void SyncWithRegistry(GameData data)
    {
        EnsureRegistry();
        if (_registry == null || data == null) return;

        // добавляем отсутствующие квесты в сейв (стартовые значения)
        foreach (var def in _registry.quests)
        {
            if (def == null || string.IsNullOrEmpty(def.id)) continue;
            if (data.quests.All(q => q.id != def.id))
                data.quests.Add(new QuestStateDTO
                {
                    id = def.id,
                    tradedGold = def.baseGold,
                    status = QuestStatus.NotReceived
                });
        }
    }

    // ---------- Read/Update ----------
    public static QuestStateDTO GetState(GameData data, string id) => data.quests.FirstOrDefault(q => q.id == id);

    public static void Update(GameData data, string id, Action<QuestStateDTO> upd)
    {
        var q = GetState(data, id);
        if (q != null) upd(q);
    }

    // ---------- Unlock logic (second condition for quests) ----------
    // Условие разблокировки квеста: ВСЕ его unlockConditions должны быть в completedConditions.
    // Если список unlockConditions пуст — квест доступен.
    public static bool IsQuestUnlocked(string questId)
    {
        EnsureRegistry();
        var def = GetDef(questId);
        if (def == null) return false;

        var conds = def.unlockConditions;
        if (conds == null || conds.Count == 0) return true;

        var completed = GameRepository.Data.completedConditions;
        return conds.All(c => c != null && completed.Contains(c.conditionId));
    }

    public static List<QuestStateDTO> GetUnlockedQuestStates()
        => GameRepository.Data.quests.Where(q => IsQuestUnlocked(q.id)).ToList();

    // ---------- Event bridge ----------
    // Подписываемся на общую шину событий и отмечаем выполненные ConditionId,
    // чтобы квесты сработали от тех же событий, что и посетители.
    public static void SubscribeIfNeeded()
    {
        if (_subscribed) return;
        _subscribed = true;

        GameEvents.OnQuestCompleted      += id => TryCompleteByEvent(ConditionType.QuestCompleted, id);
        GameEvents.OnBuildingConstructed += id => TryCompleteByEvent(ConditionType.BuildingConstructed, id);
        GameEvents.OnItemPurchased       += id => TryCompleteByEvent(ConditionType.ItemPurchased, id);
    }

    private static void TryCompleteByEvent(ConditionType type, string targetId)
    {
        EnsureRegistry();
        if (_registry == null) return;

        bool changed = false;
        var completed = GameRepository.Data.completedConditions;

        foreach (var def in _registry.quests)
        {
            if (def == null || def.unlockConditions == null) continue;

            foreach (var c in def.unlockConditions)
            {
                if (c == null) continue;
                if (completed.Contains(c.conditionId)) continue;

                if (c.type == type && c.targetId == targetId)
                {
                    completed.Add(c.conditionId);
                    changed = true;
                }
            }
        }

        if (changed) GameRepository.Save();
    }
}
