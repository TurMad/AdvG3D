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
            AdvanceQuestTravelProgressOneHour();

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
    }

    void UpdateUI()
    {
        if (dayText != null)
            dayText.text = GameRepository.Data.day.ToString();

        if (hourText != null)
            hourText.text = $"{GameRepository.Data.hour}:00";
    }
    
    void AdvanceQuestTravelProgressOneHour()
    {
        var data = GameRepository.Data;
        if (data == null || data.quests == null) return;

        foreach (var qs in data.quests)
        {
            if (qs == null) continue;
            if (qs.status != QuestStatus.InProgress) continue;
          
            qs.travelElapsedSeconds += realSecondsPerGameHour;
        }
    }
}