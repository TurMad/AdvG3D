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
    [SerializeField] float realSecondsPerGameHour = 5f; // скорость: каждые 5 секунд +1 час

    float timer;

    void Start()
    {
        // нормализуем
        if (GameRepository.Data.hour < startHour)
            GameRepository.Data.hour = startHour;

        UpdateUI();
        nextDayButton.interactable = false;
    }

    void Update()
    {
        if (GameRepository.Data.hour >= endHour)
        {
            nextDayButton.interactable = true;
            return;
        }

        timer += Time.deltaTime;
        if (timer >= realSecondsPerGameHour)
        {
            timer = 0;
            GameRepository.Data.hour++;

            if (GameRepository.Data.hour >= endHour)
                GameRepository.Data.hour = endHour;

            UpdateUI();
        }
    }

    public void OnClickNextDay()
    {
        if (GameRepository.Data.hour < endHour) return;

        GameRepository.Data.day++;
        GameRepository.Data.hour = startHour;

        nextDayButton.interactable = false;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (dayText != null)
            dayText.text = GameRepository.Data.day.ToString();

        if (hourText != null)
            hourText.text = $"{GameRepository.Data.hour}:00";
    }
}