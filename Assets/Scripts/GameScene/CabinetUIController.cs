using System.Collections.Generic;
using UnityEngine;

public class CabinetUIController : MonoBehaviour
{
    [Header("Prefabs & Parents")]
    [SerializeField] private GameObject questSendWindowPrefab;   
    [SerializeField] private Transform windowsParent;   

    [Header("Panels")]
    [SerializeField] private GameObject windowsContainer;    

    private readonly List<QuestSendWindow> _windows = new();
    private int _currentWindowIndex = 0;

    private void OnEnable()
    {
        BuildQuestWindowsList();
    }
    private void OnDisable()
    {
        ClearQuestWindowsList();
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
        if (windowsContainer != null)
            windowsContainer.SetActive(false);
    }
    
    public void OnClick_NextQuestWindow()
    {
        if (_windows.Count == 0) return;

        // круговая прокрутка вперёд
        _currentWindowIndex = (_currentWindowIndex + 1) % _windows.Count;
        UpdateWindowsVisibility();
    }

    public void OnClick_PreviousQuestWindow()
    {
        if (_windows.Count == 0) return;

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

        foreach (var questState in data.quests)
        {
            if (questState == null) continue;
            if (questState.status != QuestStatus.Received) continue;

            // создаём инстанс префаба
            var go = Instantiate(questSendWindowPrefab, windowsParent);
            var window = go.GetComponent<QuestSendWindow>();
            if (window != null)
            {
                window.questId = questState.id;
                _windows.Add(window);
            }

            go.SetActive(false);
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
    }
}
