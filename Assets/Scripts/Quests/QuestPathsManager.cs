using UnityEngine;
using System.Collections.Generic;

public class QuestPathsManager : MonoBehaviour
{
    public static QuestPathsManager Instance;

    [SerializeField] QuestPath[] paths;
    [SerializeField] private float secondsPerGameHour = 5f;

    Dictionary<string, QuestPath> cachedPaths = new();

    void Awake()
    {
        Instance = this;

        foreach (var p in paths)
        {
            if (p != null && !string.IsNullOrEmpty(p.pathId))
            {
                cachedPaths[p.pathId] = p;
                p.gameObject.SetActive(false);   
            }
        }
    }
    
    void Start()
    {
        if (GameRepository.Data == null)
            GameRepository.InitOrLoad();
        ActivateInProgressPathsOnStart();    
    }
    void ActivateInProgressPathsOnStart()
    {
        var data = GameRepository.Data;
        if (data == null || data.quests == null) return;

        foreach (var qs in data.quests)
        {
            if (qs == null) continue;
            if (qs.status != QuestStatus.InProgress) continue;

            if (cachedPaths.ContainsKey(qs.id))
            {
                ActivatePath(qs.id);
            }
        }
    }

    public void ActivatePath(string questId)
    {
        if (cachedPaths.TryGetValue(questId, out var path))
        {
            Debug.Log($"[QuestPathsManager] Activating path: {questId}");
        
            path.gameObject.SetActive(true); 

            var mover = path.GetComponent<QuestPath>();
            if (mover != null && mover.SplineAnimate != null)
            {
                var def = QuestService.GetDef(questId);
                if (def != null)
                {
                    float durationSeconds = def.travelHours * secondsPerGameHour;
                    mover.SplineAnimate.Duration = durationSeconds;
                }

                var state = QuestService.GetState(GameRepository.Data, questId);
                if (state != null)
                {
                    mover.SplineAnimate.ElapsedTime = state.travelElapsedSeconds;
                }
            }
        }
        else
        {
            Debug.LogWarning($"[QuestPathsManager] Path NOT FOUND for questId: {questId}");
        }
    }
    
    public void PauseAllPaths()
    {
        foreach (var kvp in cachedPaths)
        {
            var mover = kvp.Value.GetComponent<QuestPath>();
            if (mover != null && mover.SplineAnimate != null)
            {
                mover.SplineAnimate.Pause();
            }
        }
    }

    public void ResumeAllPaths()
    {
        foreach (var kvp in cachedPaths)
        {
            var mover = kvp.Value.GetComponent<QuestPath>();
            if (mover != null && mover.SplineAnimate != null)
            {
                mover.SplineAnimate.Play();
            }
        }
    }
}