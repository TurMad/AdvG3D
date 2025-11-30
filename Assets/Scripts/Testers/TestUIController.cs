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
        VisitorService.SubscribeIfNeeded();
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
}