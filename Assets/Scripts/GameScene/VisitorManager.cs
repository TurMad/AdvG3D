using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VisitorManager : MonoBehaviour
{
    public static VisitorManager Instance { get; private set; }

    [Header("Registry")]
    [SerializeField] private VisitorRegistry registry;

    public List<VisitorStateDTO> todayVisitors = new();

    private void Awake()
    {
        Instance = this;

        if (registry == null)
        {
            VisitorService.EnsureRegistry();
            registry = Resources.Load<VisitorRegistry>("VisitorRegistry");
        }
    }
    public void GenerateVisitorsForToday()
    {
        todayVisitors.Clear();

        GenerateMainVisitorForToday();
        // потом добавим:
        // GenerateQuestGiversForToday();
        // GenerateTradersForToday();
        // GenerateMiscVisitorsForToday();
    }
    
    private void GenerateMainVisitorForToday()
    {
        var data = GameRepository.Data;
        if (data == null) return;

        if (data.visitors == null)
            data.visitors = new List<VisitorStateDTO>();

        // 1. Берём все стейты визиторов со статусом Доступен
        var candidateStates = data.visitors
            .Where(vs => vs.status == VisitorStatus.Available)
            .ToList();

        if (candidateStates.Count == 0) return;

        // 2. Соотносим с дефинициями и фильтруем по типу Main + репутация + условия
        var candidates =
            (from vs in candidateStates
             let def = GetDefinition(vs.id)
             where def != null
                   && def.kind == VisitorKind.MainQuest
                   && IsVisitorAvailableByConditions(def)
             select (def, vs)).ToList();

        if (candidates.Count == 0) return;

        // Пока берём первого подходящего
        var (selectedDef, selectedState) = candidates[0];

        // 3. Ищем доступный квест из списка этого визитора
        if (selectedDef.questIds == null || selectedDef.questIds.Count == 0)
        {
            Debug.LogWarning($"[VisitorManager] Main visitor '{selectedDef.id}' has no questIds assigned.");
            return;
        }

        QuestStateDTO selectedQuestState = null;

        foreach (var questId in selectedDef.questIds)
        {
            var qs = QuestService.GetState(GameRepository.Data, questId);
            if (qs == null) continue;

            // считаем "доступным" только NotReceived
            if (qs.status == QuestStatus.NotReceived)
            {
                selectedQuestState = qs;
                break;
            }
        }

        if (selectedQuestState == null)
        {
            // у этого визитора нет доступных квестов
            return;
        }

        // 4. Ставим статусы в очередь
        selectedState.status = VisitorStatus.InQueue;

        selectedQuestState.status = QuestStatus.InQueue;

        todayVisitors.Add(selectedState);

        GameRepository.Save();
    }

    public void RestoreTodayVisitorsFromData()
    {
        todayVisitors.Clear();

        var data = GameRepository.Data;
        if (data == null || data.visitors == null) return;

        // Берём всех, кто уже в очереди
        var inQueue = data.visitors
            .Where(v => v.status == VisitorStatus.InQueue)
            .ToList();

        if (inQueue.Count == 0) return;

        // сортируем по типу визитора
        var ordered = inQueue
            .OrderBy(v =>
            {
                var def = GetDefinition(v.id);
                if (def == null) return 100;

                return def.kind switch
                {
                    VisitorKind.MainQuest  => 0,
                    VisitorKind.QuestGiver => 1,
                    VisitorKind.Trader     => 2,
                    VisitorKind.Misc       => 3,
                    _                      => 100
                };
            })
            .ToList();

        todayVisitors.AddRange(ordered);
    }

    private VisitorDefinition GetDefinition(string id)
    {
        if (registry == null)
        {
            VisitorService.EnsureRegistry();
            registry = Resources.Load<VisitorRegistry>("VisitorRegistry");
        }

        if (registry == null || registry.visitors == null) return null;
        return registry.visitors.FirstOrDefault(v => v != null && v.id == id);
    }

    private bool IsVisitorAvailableByConditions(VisitorDefinition def)
    {
        var data = GameRepository.Data;
        if (data == null) return false;

        if (data.reputation < def.requiredReputation)
            return false;

        if (def.conditions == null || def.conditions.Count == 0)
            return true;

        var completed = data.completedConditions;
        foreach (var c in def.conditions)
        {
            if (c == null) continue;
            if (!completed.Contains(c.conditionId)) return false;
        }

        return true;
    }
}
