using System;
using PixelCrushers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Основные")]
    [SerializeField] private Button startButton;
    [SerializeField] Button settingsButton;  
    [SerializeField] Button exitButton;
    
    [SerializeField] private GameObject slotsPanel;
    [SerializeField] private string gameScene = "Game";
    
    [SerializeField] private Button slotBtn1, slotBtn2, slotBtn3, slotBtn4;
    [SerializeField] private TMP_Text slotLabel1, slotLabel2, slotLabel3, slotLabel4;
    
    [Header("Удаление слота")]
    
    [SerializeField] private Button delBtn1;
    [SerializeField] private Button delBtn2;
    [SerializeField] private Button delBtn3;
    [SerializeField] private Button delBtn4;
    [SerializeField] private GameObject confirmPopup;
    [SerializeField] private TMP_Text confirmText;
    [SerializeField] private Button yesBtn, noBtn;
    
    private int pendingDeleteSlot = -1;
    
    [Header("Settings UI")]
    [SerializeField] GameObject settingsPanel;
    [SerializeField] TMP_Dropdown languageDropdown;
    
    [SerializeField] UILocalizationManager uiLocalizationManager;
    [SerializeField] string[] languageCodesByIndex = { "Default", "ru", "es" };

   

    private void Start()
    {
       
        slotsPanel.SetActive(false);
        confirmPopup.SetActive(false);
        settingsPanel.SetActive(false);
        startButton.onClick.AddListener(OpenSlots);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        exitButton.onClick.AddListener(OnExitClicked);
        languageDropdown.onValueChanged.AddListener(OnLanguageDropdownChanged);
        

        slotBtn1.onClick.AddListener(()=>PickSlot(1));
        slotBtn2.onClick.AddListener(()=>PickSlot(2));
        slotBtn3.onClick.AddListener(()=>PickSlot(3));
        slotBtn4.onClick.AddListener(()=>PickSlot(4));
        
        delBtn1.onClick.AddListener(()=>AskDelete(1));
        delBtn2.onClick.AddListener(()=>AskDelete(2));
        delBtn3.onClick.AddListener(()=>AskDelete(3));
        delBtn4.onClick.AddListener(()=>AskDelete(4));

        yesBtn.onClick.AddListener(ConfirmDelete);
        noBtn.onClick.AddListener(CancelDelete);

        RefreshSlotLabels();
        ApplyDropdownFromManagerLanguage();
    }

    void ApplyDropdownFromManagerLanguage()
    {
        if (uiLocalizationManager == null || languageDropdown == null) return;

        string activeLang = uiLocalizationManager.currentLanguage; 
       

        int idx = FindIndexByLangCode(activeLang);
        if (idx < 0) idx = 0; // fallback

        languageDropdown.SetValueWithoutNotify(idx);
    }
    
    int FindIndexByLangCode(string langCode)
    {
        for (int i = 0; i < languageCodesByIndex.Length; i++)
        {
            if (languageCodesByIndex[i] == langCode)
                return i;
        }
        return -1;
    }
    
    void OnLanguageDropdownChanged(int newIndex)
    {
        if (uiLocalizationManager == null) return;
        if (newIndex < 0 || newIndex >= languageCodesByIndex.Length) return;

        string newLang = languageCodesByIndex[newIndex];
        Debug.Log("[MainMenuController] user picked lang: " + newLang);

        uiLocalizationManager.currentLanguage = newLang;

    }

    private void OpenSlots()
    {
        RefreshSlotLabels();
        slotsPanel.SetActive(true);
    }

    private void RefreshSlotLabels()
    {
        slotLabel1.text = GameRepository.GetSlotLabel(1);
        slotLabel2.text = GameRepository.GetSlotLabel(2);
        slotLabel3.text = GameRepository.GetSlotLabel(3);
        slotLabel4.text = GameRepository.GetSlotLabel(4);
        
        delBtn1.gameObject.SetActive(GameRepository.SlotHasSave(1));
        delBtn2.gameObject.SetActive(GameRepository.SlotHasSave(2));
        delBtn3.gameObject.SetActive(GameRepository.SlotHasSave(3));
        delBtn4.gameObject.SetActive(GameRepository.SlotHasSave(4));
    }

    private void PickSlot(int i)
    {
        GameRepository.NewOrContinue(i);   
        SceneManager.LoadScene(gameScene); 
    }
    
    private void AskDelete(int slot)
    {
        pendingDeleteSlot = slot;
        if (confirmText != null)
            confirmText.text = $"Удалить слот {slot}? Это действие необратимо.";
        confirmPopup.SetActive(true);
    }

    private void ConfirmDelete()
    {
        if (pendingDeleteSlot > 0)
        {
            GameRepository.DeleteSlot(pendingDeleteSlot);
            pendingDeleteSlot = -1;
            confirmPopup.SetActive(false);
            RefreshSlotLabels();
        }
    }

    private void CancelDelete()
    {
        pendingDeleteSlot = -1;
        confirmPopup.SetActive(false);
    }
    
     void OnSettingsClicked()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    void OnExitClicked()
    {
        Debug.Log("[MainMenuController] Exit pressed");
        Application.Quit();
    }
    
}