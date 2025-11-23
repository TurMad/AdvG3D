using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestDetailsUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text tradedGoldText;
    [SerializeField] private TMP_Text statusText;

    [Header("Buttons")]
    [SerializeField] private Button completeButton;           // уже был
    [SerializeField] private TMP_Text completeButtonLabel;

    [SerializeField] private Button startButton;              // НОВОЕ: кнопка "отправить"
    [SerializeField] private TMP_Text startButtonLabel;       // подпись на кнопке

    // Список посетителей из второй панели (чтобы обновить)
    [SerializeField] private VisitorListUI visitorsList;
    [SerializeField] private QuestListUI questListUI;

    private string _questId;

    public void Bind(string questId)
    {
        _questId = questId;

        var def = QuestService.GetDef(_questId);
        var st  = QuestService.GetState(GameRepository.Data, _questId);

        titleText.text      = def.title;
        tradedGoldText.text = $"Золото (после торгов): {st.tradedGold}";
        statusText.text     = $"Статус: {st.status}";

        // --- Кнопка "Задание выполнено" ---
        if (completeButtonLabel) completeButtonLabel.text = "Задание выполнено";
        completeButton.onClick.RemoveAllListeners();
        completeButton.onClick.AddListener(OnCompleteClicked);

        // --- Кнопка "Отправить на задание" ---
        if (startButtonLabel) startButtonLabel.text = "Отправить на задание";
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(OnStartClicked);

        // при желании можно включать/выключать кнопки по статусу
        // например:
        //startButton.interactable   = st.status == QuestStatus.Received;
        //completeButton.interactable = st.status == QuestStatus.InProgress;
    }

    private void OnStartClicked()
    {
        var st = QuestService.GetState(GameRepository.Data, _questId);
        if (st == null) return;

        st.status = QuestStatus.InProgress;
        GameRepository.Save();

        // активируем путь для этого квеста
        if (QuestPathsManager.Instance != null)
        {
            QuestPathsManager.Instance.ActivatePath(_questId);
        }
        else
        {
            Debug.LogWarning("[QuestDetailsUI] QuestPathsManager.Instance is null");
        }

        // обновляем UI
        if (visitorsList) visitorsList.Rebuild();
        if (questListUI)  questListUI.ReBuildList();
        Bind(_questId);
    }

    private void OnCompleteClicked()
    {
        var st = QuestService.GetState(GameRepository.Data, _questId);
        if (st == null) return;

        st.status = QuestStatus.Completed;

        GameRepository.Save();
        GameEvents.RaiseQuestCompleted(_questId);

        if (visitorsList) visitorsList.Rebuild();
        questListUI.ReBuildList();
        Bind(_questId);
    }
}
