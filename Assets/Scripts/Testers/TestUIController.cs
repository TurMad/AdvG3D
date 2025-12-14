using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestUIController : MonoBehaviour
{
    [SerializeField] TMP_Text reputationText;
    [SerializeField] Button addRepButton;
    [SerializeField] int addRepStep = 50;
    [SerializeField] VisitorListUI visitorList;
    
    [SerializeField] GameObject somePanelToShow;

    void Start()
    {
        GameRepository.InitOrLoad();
        VisitorService.EnsureRegistry();
        QuestService.EnsureRegistry();
        QuestService.SubscribeIfNeeded();
        RefreshAll();

        if (addRepButton)
            addRepButton.onClick.AddListener(() =>
            {
                GameRepository.Data.reputation += addRepStep;
                GameRepository.Save();
                RefreshAll();
            });
    }
    
    public void AcceptQuest(string questId)
    {
        Debug.Log("[TestUIController] AcceptQuest " + questId);

        // 1. меняем статус квеста внутри нашей системы
        // Предполагаю, что у нас есть QuestService и он умеет обновлять стейт.
        // Если у тебя метод называется иначе — используй своё имя.
        QuestService.Update(GameRepository.Data, questId, q =>
        {
            q.status = QuestStatus.InTravelTo;
        });


        // 2. обновляем UI (включаем нужный блок интерфейса)
        if (somePanelToShow != null)
        {
            somePanelToShow.SetActive(true);
        }

    }

    void RefreshAll()
    {
        if (reputationText) reputationText.text = GameRepository.Data.reputation.ToString();
        if (visitorList) visitorList.Rebuild();
    }
    
    public void OnClick_ResetAllQuestsAndVisitors()
    {
        if (GameRepository.Data == null)
            GameRepository.InitOrLoad();

        var data = GameRepository.Data;
        if (data == null) return;

        if (data.quests != null)
        {
            foreach (var q in data.quests)
            {
                if (q == null) continue;

                q.status = QuestStatus.Received;

                q.assignedAdventurer1 = null;
                q.assignedAdventurer2 = null;
                q.assignedAdventurer3 = null;
                q.assignedAdventurer4 = null;
                q.assignedAdventurer5 = null;
            }
        }

        if (data.visitors != null)
        {
            foreach (var v in data.visitors)
            {
                if (v == null) continue;
                v.status = VisitorStatus.Available;
            }
        }

        if (data.adventurers != null)
        {
            foreach (var a in data.adventurers)
            {
                if (a == null) continue;

                a.status = AdventurerStatus.Available;
            }
        }

        if (VisitorManager.Instance != null)
            VisitorManager.Instance.todayVisitors.Clear();

        GameRepository.Save();

    }
}