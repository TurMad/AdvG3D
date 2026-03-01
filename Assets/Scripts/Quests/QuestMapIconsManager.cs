using System.Collections.Generic;
using UnityEngine;

public class QuestMapIconsManager : MonoBehaviour
{
    public static QuestMapIconsManager Instance { get; private set; }

    [SerializeField] private QuestMapIconItem[] icons;

    private readonly Dictionary<string, QuestMapIconItem> _byQuestId = new();

    private void Awake()
    {
        Instance = this;

        _byQuestId.Clear();

        if (icons != null)
        {
            foreach (var icon in icons)
            {
                if (icon == null) continue;
                if (string.IsNullOrEmpty(icon.QuestId)) continue;

                _byQuestId[icon.QuestId] = icon;
                icon.SetVisible(false);
            }
        }
    }

    private void Start()
    {
        RefreshFromData();
    }

    public void RefreshFromData()
    {
        foreach (var kv in _byQuestId)
            kv.Value.SetVisible(false);

        var data = GameRepository.Data;
        if (data == null || data.quests == null) return;

        foreach (var qs in data.quests)
        {
            if (qs == null) continue;
            if (qs.status != QuestStatus.Received) continue;

            if (_byQuestId.TryGetValue(qs.id, out var icon))
                icon.SetVisible(true);
        }
    }

    public void ShowIcon(string questId)
    {
        if (_byQuestId.TryGetValue(questId, out var icon))
            icon.SetVisible(true);
    }

    public void HideIcon(string questId)
    {
        if (_byQuestId.TryGetValue(questId, out var icon))
            icon.SetVisible(false);
    }
}