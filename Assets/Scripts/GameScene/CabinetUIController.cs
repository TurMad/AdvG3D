using System.Collections.Generic;
using UnityEngine;

public class CabinetUIController : MonoBehaviour
{
    public static CabinetUIController Instance { get; private set; }

    [Header("Adventurer Carousel")]
    [SerializeField] private AdventurerCarouselController adventurerCarousel;
    
    [Header("Prefabs & Parents")]
    [SerializeField] private GameObject questSendWindowPrefab;   
    [SerializeField] private Transform windowsParent;   

    [Header("Panels")]
    [SerializeField] private GameObject windowsContainer;    
    
    [Header("Adventurer Card Prefabs (global)")]
    [SerializeField] private AdventurerCard[] adventurerCardPrefabs;

    public AdventurerCard[] GetAdventurerCardPrefabs() => adventurerCardPrefabs;

    private readonly List<QuestSendWindow> _windows = new();
    private int _currentWindowIndex = 0;

    private void Awake()
    {
        Instance = this;
    }
    private void OnEnable()
    {
        BuildQuestWindowsList();
    }
    private void OnDisable()
    {
        ClearQuestWindowsList();
    }
    
    public void OpenAdventurerCarousel(QuestSendWindow window, int slotIndex)
    {
        if (adventurerCarousel == null) return;

        adventurerCarousel.Show(window, slotIndex);
    }
    
    public void CloseAdventurerCarousel()
    {
        if (adventurerCarousel == null) return;
        adventurerCarousel.Hide();
    }
    
    public void OnClick_OpenQuestWindows()
    {
        if (windowsContainer != null)
            windowsContainer.SetActive(true);
        if (_windows.Count > 0)
        {
            _currentWindowIndex = 0;
            UpdateWindowsVisibility();
        }
    }
    
    public void OnClick_CloseQuestWindows()
    {
        _windows[_currentWindowIndex].ResetAndReleaseSelectedAdventurers();
        if (windowsContainer != null)
            windowsContainer.SetActive(false);
    }
    
    public void OnClick_NextQuestWindow()
    {
        if (_windows.Count == 0) return;
        _windows[_currentWindowIndex].ResetAndReleaseSelectedAdventurers();
        // круговая прокрутка вперёд
        _currentWindowIndex = (_currentWindowIndex + 1) % _windows.Count;
        UpdateWindowsVisibility();
    }

    public void OnClick_PreviousQuestWindow()
    {
        if (_windows.Count == 0) return;
        _windows[_currentWindowIndex].ResetAndReleaseSelectedAdventurers();
        // круговая прокрутка назад
        _currentWindowIndex = (_currentWindowIndex - 1 + _windows.Count) % _windows.Count;
        UpdateWindowsVisibility();
    }
    
    private void UpdateWindowsVisibility()
    {
        for (int i = 0; i < _windows.Count; i++)
        {
            if (_windows[i] == null) continue;
            _windows[i].gameObject.SetActive(i == _currentWindowIndex);
        }
    }

    private void BuildQuestWindowsList()
    {
        ClearQuestWindowsList();

        var data = GameRepository.Data;
        if (data == null || data.quests == null) return;
        
        if (data.adventurers != null)
        {
            bool changed = false;

            foreach (var adv in data.adventurers)
            {
                if (adv == null) continue;

                // Временный статус выбора — сбрасываем
                if (adv.status == AdventurerStatus.Selected)
                {
                    adv.status = AdventurerStatus.Available;
                    changed = true;
                }
            }

            if (changed)
                GameRepository.Save();
        }

        foreach (var questState in data.quests)
        {
            if (questState == null) continue;
            if (questState.status != QuestStatus.Received) continue;

            CreateQuestWindow(questState.id);
        }

        // 2) ПОТОМ: в процессе (3 статуса)
        foreach (var questState in data.quests)
        {
            if (questState == null) continue;

            bool isInProgress =
                questState.status == QuestStatus.InTravelTo ||
                questState.status == QuestStatus.InExecution ||
                questState.status == QuestStatus.InTravelBack;

            if (!isInProgress) continue;

            CreateQuestWindow(questState.id);
        }
    }

    private void CreateQuestWindow(string id)
    {
        var go = Instantiate(questSendWindowPrefab, windowsParent);
        var window = go.GetComponent<QuestSendWindow>();
        if (window != null)
        {
            window.questId = id;
            _windows.Add(window);
        }

        go.SetActive(false);
    }

    private void ClearQuestWindowsList()
    {
        for (int i = 0; i < _windows.Count; i++)
        {
            if (_windows[i] != null)
                Destroy(_windows[i].gameObject);
        }

        _windows.Clear();
    }
}
