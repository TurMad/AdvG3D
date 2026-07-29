using System.Collections.Generic;
using UnityEngine;

public class QuestSendUIController : MonoBehaviour
{
    public static QuestSendUIController Instance { get; private set; }

    [Header("Adventurer Carousel")]
    [SerializeField] private CardViewController cabinetCardView;
    [SerializeField] private CardViewController mapCardView;

    [Header("Prefabs")]
    [SerializeField] private GameObject questSendWindowPrefab;
    
    [SerializeField] private MapCameraInputController mapCameraInput;

    [Header("Cabinet Panel (List)")]
    [SerializeField] private Transform windowsParent;
    [SerializeField] private GameObject windowsContainer;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject prevButton;

    [Header("Map Panel (Single Quest)")]
    [SerializeField] private Transform mapWindowsParent;
    [SerializeField] private GameObject mapWindowsContainer;

    [Header("Adventurer Card Prefabs (global)")]
    [SerializeField] private AdventurerCard[] adventurerCardPrefabs;

    public AdventurerCard[] GetAdventurerCardPrefabs() => adventurerCardPrefabs;

    private readonly List<QuestSendWindow> _windows = new();
    private int _currentWindowIndex = 0;

    private QuestSendWindow _mapWindow;

    private void Awake()
    {
        Instance = this;
    }

    // ===== Carousel =====
    public void OpenAdventurerCarousel(QuestSendWindow window, int slotIndex)
    {
        var carousel = GetActiveCarousel();
        if (carousel == null) return;

        carousel.Show(window, slotIndex);
    }

    public void CloseAdventurerCarousel()
    {
        var carousel = GetActiveCarousel();
        if (carousel == null) return;

        carousel.Hide();
    }
    
    private CardViewController GetActiveCarousel()
    {
        // если открыта MAP панель — используем map карусель, иначе cabinet
        if (mapWindowsContainer != null && mapWindowsContainer.activeInHierarchy)
            return mapCardView;

        return cabinetCardView;
    }

    // ===== CABINET: list of quests (Received + InProgress) =====
    public void OnClick_OpenQuestWindows()
    {
        BuildQuestWindowsList();
        UpdateNavButtons();

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
        if (_windows.Count > 0 && _currentWindowIndex >= 0 && _currentWindowIndex < _windows.Count)
        {
            var cur = _windows[_currentWindowIndex];
            if (cur != null && IsQuestReceived(cur.questId))
                cur.ResetAndReleaseSelectedAdventurers();
        }

        if (windowsContainer != null)
            windowsContainer.SetActive(false);

        ClearQuestWindowsList();
    }

    public void OnClick_NextQuestWindow()
    {
        if (_windows.Count == 0) return;

        var cur = _windows[_currentWindowIndex];
        if (cur != null && IsQuestReceived(cur.questId))
            cur.ResetAndReleaseSelectedAdventurers();

        _currentWindowIndex = (_currentWindowIndex + 1) % _windows.Count;
        UpdateWindowsVisibility();
        UpdateNavButtons();
    }

    public void OnClick_PreviousQuestWindow()
    {
        if (_windows.Count == 0) return;

        var cur = _windows[_currentWindowIndex];
        if (cur != null && IsQuestReceived(cur.questId))
            cur.ResetAndReleaseSelectedAdventurers();

        _currentWindowIndex = (_currentWindowIndex - 1 + _windows.Count) % _windows.Count;
        UpdateWindowsVisibility();
        UpdateNavButtons();
    }
    
    private void UpdateNavButtons()
    {
        bool show = _windows.Count > 1;

        if (nextButton != null) nextButton.SetActive(show);
        if (prevButton != null) prevButton.SetActive(show);
    }
    
    private bool IsQuestReceived(string questId)
    {
        var data = GameRepository.Data;
        if (data == null) return false;

        var qs = QuestService.GetState(data, questId);
        return qs != null && qs.status == QuestStatus.Received;
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

        // 0) сбрасываем временный Selected -> Available (как у тебя было)
        if (data.adventurers != null)
        {
            bool changed = false;

            foreach (var adv in data.adventurers)
            {
                if (adv == null) continue;

                if (adv.status == AdventurerStatus.Selected)
                {
                    adv.status = AdventurerStatus.Available;
                    changed = true;
                }
            }

            if (changed)
                GameRepository.Save();
        }

        // 1) сначала Received
        foreach (var questState in data.quests)
        {
            if (questState == null) continue;
            if (questState.status != QuestStatus.Received) continue;

            CreateQuestWindow(questState.id, windowsParent, _windows, inactiveByDefault: true);
        }

        // 2) потом InProgress
        foreach (var questState in data.quests)
        {
            if (questState == null) continue;

            bool isInProgress =
                questState.status == QuestStatus.InTravelTo ||
                questState.status == QuestStatus.InExecution ||
                questState.status == QuestStatus.InTravelBack;

            if (!isInProgress) continue;

            CreateQuestWindow(questState.id, windowsParent, _windows, inactiveByDefault: true);
        }
    }

    private void ClearQuestWindowsList()
    {
        for (int i = 0; i < _windows.Count; i++)
        {
            if (_windows[i] != null)
                Destroy(_windows[i].gameObject);
        }

        _windows.Clear();
        _currentWindowIndex = 0;
    }

    // ===== MAP: single quest window =====
    public void OpenMapQuestWindow(string questId)
    {
        CloseMapQuestWindow();

        if (mapWindowsContainer != null)
            mapWindowsContainer.SetActive(true);

        _mapWindow = CreateQuestWindow(questId, mapWindowsParent, true);
        if (_mapWindow != null)
            _mapWindow.gameObject.SetActive(true);

        if (mapCameraInput != null)
        {
            mapCameraInput.enabled = false;
        }
            
    }

    public void CloseMapQuestWindow()
    {
        if (_mapWindow != null)
        {
            _mapWindow.ResetAndReleaseSelectedAdventurers();
            Destroy(_mapWindow.gameObject);
            _mapWindow = null;
        }

        if (mapWindowsContainer != null)
            mapWindowsContainer.SetActive(false);
        
        if (mapCameraInput != null)
            mapCameraInput.enabled = true;
    }

    // ===== factory =====
    private QuestSendWindow CreateQuestWindow(string id, Transform parent, List<QuestSendWindow> addToList, bool inactiveByDefault)
    {
        if (questSendWindowPrefab == null || parent == null) return null;

        var go = Instantiate(questSendWindowPrefab, parent);
        var window = go.GetComponent<QuestSendWindow>();

        if (window != null)
        {
            window.questId = id;
            addToList?.Add(window);
        }

        if (inactiveByDefault)
            go.SetActive(false);

        return window;
    }
    
    private QuestSendWindow CreateQuestWindow(string id, Transform parent, bool makeActive)
    {
        var go = Instantiate(questSendWindowPrefab, parent);

        // ВАЖНО: выключаем сразу, чтобы OnEnable не сработал раньше времени
        go.SetActive(false);

        var window = go.GetComponent<QuestSendWindow>();
        if (window != null)
            window.questId = id;

        if (makeActive)
            go.SetActive(true);

        return window;
    }
}