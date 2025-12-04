using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VisitorManager : MonoBehaviour
{
    public static VisitorManager Instance { get; private set; }

    [Header("Registry")]
    [SerializeField] private VisitorRegistry registry;
    
    [Header("Per-day limits")]
    [SerializeField] private int maxQuestGiversPerDay = 2;
    [SerializeField] private int maxMiscPerDay = 2;

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
        var data = GameRepository.Data;
        if (data == null)
        {
            GameRepository.InitOrLoad();
            data = GameRepository.Data;
            if (data == null) return;
        }
        var availableDefs = VisitorService.GetAvailableVisitors();
        var candidates = new List<VisitorCandidate>();
        foreach (var def in availableDefs)
        {
            if (def == null) continue;

            var state = data.visitors.FirstOrDefault(v => v.id == def.id);
            if (state == null) continue;
            if (state.status != VisitorStatus.Available)
                continue;
            bool hasQuest = false;
            if (def.questIds != null)
            {
                foreach (var questId in def.questIds)
                {
                    if (QuestService.IsQuestAvailable(data, questId))
                    {
                        hasQuest = true;
                        break;
                    }
                }
            }

            candidates.Add(new VisitorCandidate
            {
                def = def,
                state = state,
                hasAvailableQuest = hasQuest
            });
        }
        GenerateMainVisitor(candidates);
        GenerateQuestGivers(candidates, data);
        GenerateTrader(candidates);
        GenerateMisc(candidates);
        
        GameRepository.Save();
    }
    void GenerateMainVisitor(List<VisitorCandidate> candidates)
    {
        // только MainQuest, доступные и с хотя бы одним доступным квестом
        var mains = candidates
            .Where(c => c.def.kind == VisitorKind.MainQuest
                        && c.state.status == VisitorStatus.Available
                        && c.hasAvailableQuest)
            .ToList();

        if (mains.Count == 0) return;

        // можно взять первого или рандом
        var chosen = mains[Random.Range(0, mains.Count)];

        // выбираем конкретный квест для этого визитора
        if (!TrySelectQuestForVisitor(chosen.def, out var questState))
            return;

        chosen.state.status = VisitorStatus.InQueue;
        questState.status = QuestStatus.InQueue;

        todayVisitors.Add(chosen.state);
    }
    void GenerateQuestGivers(List<VisitorCandidate> candidates, GameData data)
    {
        int remainingSlots = data.maxActiveQuests - data.currentActiveQuests;
        if (remainingSlots <= 0) return;

        int allowedBySlots = Mathf.Min(remainingSlots, maxQuestGiversPerDay);
        if (allowedBySlots <= 0) return;

        // только QuestGiver + доступен + есть хотя бы один доступный квест
        var givers = candidates
            .Where(c => c.def.kind == VisitorKind.QuestGiver
                        && c.state.status == VisitorStatus.Available
                        && c.hasAvailableQuest)
            .ToList();

        if (givers.Count == 0) return;

        // лёгкий рандом
        for (int i = 0; i < givers.Count; i++)
        {
            int j = Random.Range(i, givers.Count);
            (givers[i], givers[j]) = (givers[j], givers[i]);
        }

        int spawned = 0;

        foreach (var candidate in givers)
        {
            if (spawned >= allowedBySlots)
                break;

            // выбираем конкретный квест
            if (!TrySelectQuestForVisitor(candidate.def, out var questState))
                continue;

            candidate.state.status = VisitorStatus.InQueue;
            questState.status = QuestStatus.InQueue;

            todayVisitors.Add(candidate.state);
            spawned++;

            if (data.currentActiveQuests >= data.maxActiveQuests)
                break;
        }
    }
    void GenerateTrader(List<VisitorCandidate> candidates)
    {
        var traders = candidates
            .Where(c => c.def.kind == VisitorKind.Trader
                        && c.state.status == VisitorStatus.Available)
            .ToList();

        if (traders.Count == 0) return;

        var chosen = traders[Random.Range(0, traders.Count)];

        chosen.state.status = VisitorStatus.InQueue;
        todayVisitors.Add(chosen.state);
    }
    void GenerateMisc(List<VisitorCandidate> candidates)
    {
        var miscs = candidates
            .Where(c => c.def.kind == VisitorKind.Misc
                        && c.state.status == VisitorStatus.Available)
            .ToList();

        if (miscs.Count == 0) return;

        for (int i = 0; i < miscs.Count; i++)
        {
            int j = Random.Range(i, miscs.Count);
            (miscs[i], miscs[j]) = (miscs[j], miscs[i]);
        }

        int spawned = 0;

        foreach (var candidate in miscs)
        {
            if (spawned >= maxMiscPerDay)
                break;

            candidate.state.status = VisitorStatus.InQueue;
            todayVisitors.Add(candidate.state);
            spawned++;
        }
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
    
    private bool TrySelectQuestForVisitor(VisitorDefinition visitorDef, out QuestStateDTO questState)
    {
        questState = null;
        var data = GameRepository.Data;
        if (data == null) return false;

        if (visitorDef.questIds == null || visitorDef.questIds.Count == 0)
            return false;

        foreach (var questId in visitorDef.questIds)
        {
            if (!QuestService.IsQuestAvailable(data, questId))
                continue;

            var qState = QuestService.GetState(data, questId);
            if (qState == null) continue;

            questState = qState;
            return true;
        }

        return false;
    }
    public VisitorStateDTO GetFirstTodayVisitor()
    {
        if (todayVisitors == null || todayVisitors.Count == 0)
            return null;

        return todayVisitors[0];
    }

    private struct VisitorCandidate
    {
        public VisitorDefinition def;
        public VisitorStateDTO state;
        public bool hasAvailableQuest;
    }
}


