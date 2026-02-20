using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MissionReportsController : MonoBehaviour
{
    public static MissionReportsController Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panelRoot;                 // вся панель отчётов
    [SerializeField] private Transform reportParent;               // куда инстансить карточку
    [SerializeField] private MissionReportItemUI reportItemPrefab; // префаб карточки

    // Пока делаем “один отчёт” — храним текущую карточку
    private MissionReportItemUI _spawnedItem;

    private void Awake()
    {
        Instance = this;
        if (panelRoot) panelRoot.SetActive(false);
    }

    public bool HasAnyReports()
    {
        var data = GameRepository.Data;
        return data != null && data.missionReports != null && data.missionReports.Count > 0;
    }

    /// <summary>
    /// Вызывается при завершении возврата (TravelBack закончился).
    /// Тут мы добавляем/показываем отчёт по конкретному квесту.
    /// Пока логика: берём самый последний report для questId.
    /// </summary>
    public void AddReportForQuest(string questId)
    {
        var data = GameRepository.Data;
        if (data == null || data.missionReports == null || data.missionReports.Count == 0) return;

        var report = GetLatestReportForQuest(data, questId);
        if (report == null) return;

        // Пока 1 отчёт: удаляем старый и показываем новый
        ClearSpawned();

        _spawnedItem = Instantiate(reportItemPrefab, reportParent);
        _spawnedItem.Bind(report);

        // UI пока НЕ открываем автоматически (как ты сказал),
        // просто добавили в список/панель.
    }

    public void OnClick_ShowReports()
    {
        if (!HasAnyReports()) return;

        if (panelRoot)
            panelRoot.SetActive(true);
    }

    public void OnClick_HideReports()
    {
        if (panelRoot)
            panelRoot.SetActive(false);
    }

    private void ClearSpawned()
    {
        if (_spawnedItem != null)
            Destroy(_spawnedItem.gameObject);
        _spawnedItem = null;

        if (reportParent != null)
        {
            // на всякий (если что-то руками накинут)
            for (int i = reportParent.childCount - 1; i >= 0; i--)
                Destroy(reportParent.GetChild(i).gameObject);
        }
    }

    private MissionReportDTO GetLatestReportForQuest(GameData data, string questId)
    {
        // reportId у тебя: questId_1, questId_2 ...
        // берём максимальный индекс
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

        // если вдруг reportId не по формату — fallback: последний по questId
        if (best == null)
            best = data.missionReports.LastOrDefault(x => x != null && x.questId == questId);

        return best;
    }
}
