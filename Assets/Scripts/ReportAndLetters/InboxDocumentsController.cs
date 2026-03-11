using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InboxDocumentsController : MonoBehaviour
{
    public static InboxDocumentsController Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform contentParent;

    [Header("Prefabs")]
    [SerializeField] private MissionReportItemUI reportPrefab;
    [SerializeField] private QuestLetterItemUI letterPrefab;

    [Header("Navigation")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;

    private readonly List<GameObject> _items = new();
    private int _currentIndex = 0;

    private void Awake()
    {
        Instance = this;
        if (panelRoot) panelRoot.SetActive(false);
    }

    private void OnEnable()
    {
        Rebuild();
    }

    public bool HasAnyOnDesk()
    {
        var data = GameRepository.Data;
        if (data == null) return false;

        bool hasReports = data.missionReports != null && data.missionReports.Any(r => r != null && r.status == InboxItemStatus.OnDesk);
        bool hasLetters = data.questLetters != null && data.questLetters.Any(l => l != null && l.status == InboxItemStatus.OnDesk);

        return hasReports || hasLetters;
    }

    public void OnClick_Open()
    {
        if (!HasAnyOnDesk() && _items.Count == 0) return;

        if (panelRoot) panelRoot.SetActive(true);

        if (_items.Count > 0)
        {
            ReorderItemsByStatus();                 // <-- НОВОЕ
            int firstUnread = FindFirstOnDeskIndex();
            ShowIndex(firstUnread >= 0 ? firstUnread : 0);
        }
    }

    public void OnClick_Close()
    {
        if (panelRoot) panelRoot.SetActive(false);
        // НЕ чистим тут — чистка только при OnEnable
    }

    public void OnClick_Prev()
    {
        if (_items.Count <= 1) return;
        _currentIndex = (_currentIndex - 1 + _items.Count) % _items.Count;
        ShowIndex(_currentIndex);
    }

    public void OnClick_Next()
    {
        if (_items.Count <= 1) return;
        _currentIndex = (_currentIndex + 1) % _items.Count;
        ShowIndex(_currentIndex);
    }

    public void Rebuild()
    {
        ClearContent();
        _items.Clear();
        _currentIndex = 0;

        var data = GameRepository.Data;
        if (data == null) { UpdateNavButtons(); return; }

        // 1) сначала репорты OnDesk
        if (data.missionReports != null)
        {
            foreach (var r in data.missionReports.Where(x => x != null && x.status == InboxItemStatus.OnDesk))
            {
                var item = Instantiate(reportPrefab, contentParent);
                item.Bind(r);
                item.gameObject.SetActive(false);
                _items.Add(item.gameObject);
            }
        }

        // 2) потом письма OnDesk
        if (data.questLetters != null)
        {
            foreach (var l in data.questLetters.Where(x => x != null && x.status == InboxItemStatus.OnDesk))
            {
                var item = Instantiate(letterPrefab, contentParent);
                item.Bind(l.questId);
                item.gameObject.SetActive(false);
                _items.Add(item.gameObject);
            }
        }

        UpdateNavButtons();

    }

    private void ShowIndex(int index)
    {
        for (int i = 0; i < _items.Count; i++)
            _items[i].SetActive(i == index);

        MarkCurrentAsRead(index);
        UpdateNavButtons();
    }

    private void MarkCurrentAsRead(int index)
    {
        var data = GameRepository.Data;
        if (data == null) return;

        // Определяем: текущий объект — репорт или письмо по компоненту
        var go = _items[index];

        var report = go.GetComponent<MissionReportItemUI>();
        if (report != null)
        {
            var dto = data.missionReports.FirstOrDefault(x => x != null && x.reportId == report.BoundReportId);
            if (dto != null && dto.status == InboxItemStatus.OnDesk)
            {
                dto.status = InboxItemStatus.Read;
                GameRepository.Save();
            }
            return;
        }

        var letter = go.GetComponent<QuestLetterItemUI>();
        if (letter != null)
        {
            var questId = letter.BoundQuestId;

            var dto = data.questLetters.FirstOrDefault(x => x != null && x.questId == questId && x.status == InboxItemStatus.OnDesk);
            if (dto != null)
            {
                GameSceneController.ApplyQuestLetterEffects(dto);
                dto.status = InboxItemStatus.Read;
                GameRepository.Save();
            }
        }
    }

    private void UpdateNavButtons()
    {
        bool many = _items.Count > 1;

        if (prevButton) prevButton.interactable = many;
        if (nextButton) nextButton.interactable = many;
    }

    private void ClearContent()
    {
        if (contentParent == null) return;

        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);
    }

    public void MarkLatestReportOnDesk(string questId)
    {
        var data = GameRepository.Data;
        if (data == null || data.missionReports == null) return;

        var latest = GetLatestReportForQuest(data, questId);
        if (latest == null) return;

        latest.status = InboxItemStatus.OnDesk;
        GameRepository.Save();
    }

    private MissionReportDTO GetLatestReportForQuest(GameData data, string questId)
    {
        int bestIndex = -1;
        MissionReportDTO best = null;

        foreach (var r in data.missionReports)
        {
            if (r == null) continue;
            if (r.questId != questId) continue;
            if (string.IsNullOrEmpty(r.reportId)) continue;

            string prefix = questId + "_";
            if (!r.reportId.StartsWith(prefix)) continue;

            string tail = r.reportId.Substring(prefix.Length);
            if (!int.TryParse(tail, out int idx)) continue;

            if (idx > bestIndex)
            {
                bestIndex = idx;
                best = r;
            }
        }

        if (best == null)
            best = data.missionReports.LastOrDefault(x => x != null && x.questId == questId);

        return best;
    }
    
    private int FindFirstOnDeskIndex()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (GetItemStatus(_items[i]) == InboxItemStatus.OnDesk)
                return i;
        }
        return -1;
    }

    private void ReorderItemsByStatus()
    {
        if (_items.Count <= 1) return;

        // сортируем список: OnDesk вверх, Read вниз
        _items.Sort((a, b) =>
        {
            int sa = (int)GetItemStatus(a);
            int sb = (int)GetItemStatus(b);
            return sa.CompareTo(sb);
        });

        // и синхронизируем порядок в UI (contentParent)
        for (int i = 0; i < _items.Count; i++)
            _items[i].transform.SetSiblingIndex(i);
    }
    
    private InboxItemStatus GetItemStatus(GameObject go)
    {
        var data = GameRepository.Data;
        if (data == null || go == null) return InboxItemStatus.Read;

        var reportUI = go.GetComponent<MissionReportItemUI>();
        if (reportUI != null)
        {
            var dto = data.missionReports?.FirstOrDefault(x => x != null && x.reportId == reportUI.BoundReportId);
            return dto != null ? dto.status : InboxItemStatus.Read;
        }

        var letterUI = go.GetComponent<QuestLetterItemUI>();
        if (letterUI != null)
        {
            var dto = data.questLetters?.FirstOrDefault(x => x != null && x.questId == letterUI.BoundQuestId);
            return dto != null ? dto.status : InboxItemStatus.Read;
        }

        return InboxItemStatus.Read;
    }
}