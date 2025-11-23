using UnityEngine;
using System.Collections.Generic;

public class QuestPathsManager : MonoBehaviour
{
    public static QuestPathsManager Instance;

    [SerializeField] QuestPath[] paths;

    Dictionary<string, QuestPath> cachedPaths = new();

    void Awake()
    {
        Instance = this;

        // кешируем пути для быстрого доступа
        foreach (var p in paths)
        {
            if (p != null && !string.IsNullOrEmpty(p.pathId))
            {
                cachedPaths[p.pathId] = p;
                p.gameObject.SetActive(false);   // по умолчанию все скрыты
            }
        }
    }

    public void ActivatePath(string questId)
    {
        if (cachedPaths.TryGetValue(questId, out var path))
        {
            Debug.Log($"[QuestPathsManager] Activating path: {questId}");
            path.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[QuestPathsManager] Path NOT FOUND for questId: {questId}");
        }
    }
}