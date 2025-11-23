using UnityEngine;
using TMPro;
using UnityEngine.UI;
using PixelCrushers.DialogueSystem;
using PixelCrushers;

public class GameSceneController : MonoBehaviour
{
    [Header("Language / Systems")]
    [SerializeField] UILocalizationManager uiLocalizationManager;
    [SerializeField] DialogueSystemController dialogueSystemController;
    [SerializeField] string playerPrefsLangKey = "Language"; // тот же ключ что в меню

    [Header("Header UI")]
    [SerializeField] TMP_Text goldText;
    [SerializeField] TMP_Text repText;
    [SerializeField] TMP_Text dayText;

    [SerializeField] Button receptionButton;
    [SerializeField] Button officeButton;
    [SerializeField] Button endDayButton;

    [Header("Rooms")]
    [SerializeField] GameObject receptionPanel;
    [SerializeField] GameObject officePanel;

    // временные данные дня (потом заменим на реальные из сейва)
    int currentDay = 1;
    int currentGold = 0;
    int currentRep = 0;

    void Awake()
    {
        // навешиваем кнопки
        if (receptionButton != null)
            receptionButton.onClick.AddListener(ShowReception);

        if (officeButton != null)
            officeButton.onClick.AddListener(ShowOffice);

        if (endDayButton != null)
            endDayButton.onClick.AddListener(EndDay);
    }

    void Start()
    {
        // 1. вытащить язык, который игрок выбрал в меню
        string langCode = PlayerPrefs.GetString(playerPrefsLangKey, "Default");

        // 2. применить к локализации UI
        if (uiLocalizationManager != null)
        {
            // вызови метод, который реально есть в UILocalizationManager:
            // в твоей версии это может быть SetLanguage(...) или CurrentLanguage = ...
            uiLocalizationManager.currentLanguage = langCode;
        }

        // 3. применить к Dialogue System
        if (dialogueSystemController != null)
        {
            dialogueSystemController.displaySettings.localizationSettings.language = langCode;
        }

        // 4. показать комнату по умолчанию
        ShowReception();

        // 5. обновить хедерные тексты (день/золото/реп)
        RefreshHeader();
    }

    void RefreshHeader()
    {
        if (goldText != null)
            goldText.text = currentGold.ToString();

        if (repText != null)
            repText.text = currentRep.ToString();

        if (dayText != null)
            dayText.text = "Day " + currentDay; // позже локализуем через Localize UI, а здесь можно будет убрать прямой текст
    }

    public void ShowReception()
    {
        if (receptionPanel != null) receptionPanel.SetActive(true);
        if (officePanel != null)    officePanel.SetActive(false);
    }

    public void ShowOffice()
    {
        if (receptionPanel != null) receptionPanel.SetActive(false);
        if (officePanel != null)    officePanel.SetActive(true);
    }

    public void EndDay()
    {
        // простой инкремент дня. потом сюда добавим: расчёт прибыли, спавн новых визиторов, и т.д.
        currentDay += 1;
        RefreshHeader();
    }
}
