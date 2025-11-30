using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TMP_Text dayText;
    [SerializeField] TMP_Text hourText;
    [SerializeField] Button nextDayButton;

    [Header("Config")]
    [SerializeField] int startHour = 8;
    [SerializeField] int endHour = 24;
    [SerializeField] float realSecondsPerGameHour = 5f; 

    float timer;
    

    void Start()
    {
        // нормализуем
        if (GameRepository.Data.hour < startHour)
            GameRepository.Data.hour = startHour;

        UpdateUI();
        nextDayButton.onClick.RemoveAllListeners();
        nextDayButton.onClick.AddListener(OnClickNextDay);
        nextDayButton.interactable = false;
    }

    void Update()
    {
        if (GameRepository.Data.hour >= endHour)
        {
            nextDayButton.interactable = true;
            QuestPathsManager.Instance.PauseAllPaths();
            return;
        }

        timer += Time.deltaTime;
        if (timer >= realSecondsPerGameHour)
        {
            timer = 0;
            GameRepository.Data.hour++;
            AdvanceQuestsOneHour();

            if (GameRepository.Data.hour >= endHour)
                GameRepository.Data.hour = endHour;

            UpdateUI();
            GameRepository.Save();
        }
    }

    public void OnClickNextDay()
    {
        if (GameRepository.Data.hour < endHour) return;

        GameRepository.Data.day++;
        GameRepository.Data.hour = startHour;

        nextDayButton.interactable = false;
        UpdateUI();
        QuestPathsManager.Instance.ResumeAllPaths();
        GameRepository.Save();
    }

    void UpdateUI()
    {
        if (dayText != null)
            dayText.text = GameRepository.Data.day.ToString();

        if (hourText != null)
            hourText.text = $"{GameRepository.Data.hour}:00";
    }
    
    void AdvanceQuestsOneHour()
    {
        var data = GameRepository.Data;
        if (data == null || data.quests == null) return;

        foreach (var qs in data.quests)
        {
            if (qs == null) continue;

            switch (qs.status)
            {
                case QuestStatus.InTravelTo:
                    HandleTravelTo(qs);
                    break;

                case QuestStatus.InExecution:
                    HandleExecution(qs);
                    break;

                case QuestStatus.InTravelBack:
                    HandleTravelBack(qs);   
                    break;

                default:
                    // другие статусы сейчас не трогаем
                    break;
            }
        }
        
    }

    void HandleTravelTo(QuestStateDTO qs)
    {
        qs.travelElapsedSeconds += realSecondsPerGameHour;
        var def = QuestService.GetDef(qs.id);
        if (def == null) return;
        float travelDurationSeconds = def.travelHours * realSecondsPerGameHour;
        if (qs.travelElapsedSeconds < travelDurationSeconds)
            return;
        qs.travelElapsedSeconds = travelDurationSeconds;
        qs.status = QuestStatus.InExecution;
        qs.executeHoursRemaining = def.executeHours;
        if (QuestPathsManager.Instance != null)
            QuestPathsManager.Instance.PausePathWithDelay(qs.id);
    }

    void HandleExecution(QuestStateDTO qs)
    {
        if (qs.executeHoursRemaining > 0)
            qs.executeHoursRemaining--;

        if (qs.executeHoursRemaining > 0)
            return;
        qs.executeHoursRemaining = 0;
        qs.status = QuestStatus.InTravelBack;

        if (QuestPathsManager.Instance != null)
            QuestPathsManager.Instance.ResumePath(qs.id);
    }

    void HandleTravelBack(QuestStateDTO qs)
    {
        var def = QuestService.GetDef(qs.id);
        if (def == null || QuestPathsManager.Instance == null) return;
        float travelDurationSeconds = def.travelHours * realSecondsPerGameHour;
        qs.travelElapsedSeconds += realSecondsPerGameHour;
        float fullCycle = travelDurationSeconds * 2f;
        if (qs.travelElapsedSeconds < fullCycle) return;
        qs.travelElapsedSeconds = fullCycle;
        qs.status = QuestStatus.Completed;
        QuestPathsManager.Instance.DeactivatePath(qs.id);
    }
}