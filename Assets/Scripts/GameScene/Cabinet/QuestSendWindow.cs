using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class QuestSendWindow : MonoBehaviour
{
    [Tooltip("ID квеста, для которого это окно отправки.")]
    public string questId;

    [Header("Slots UI")]
    [SerializeField] private Button[] slotButtons;        // 5 кнопок
    [SerializeField] private Transform[] slotCardParents; // 5 точек под выбранные карты

    [Header("Send")]
    [SerializeField] private Button sendButton;
    
    [Header("In Progress Blocker")]
    [SerializeField] private GameObject inProgressBlocker;

    public List<string> selectedAdventurerIds = new();

    private void OnEnable()
    {
        ResetSelectionVisualOnly();
        UpdateSlotButtonsState();
        CheckInProgress();
    }

    private void CheckInProgress()
    {
        var data = GameRepository.Data;
        if (data == null) return;

        var questState = QuestService.GetState(data, questId);
        if (questState == null) return;

        bool isInProgress =
            questState.status == QuestStatus.InTravelTo ||
            questState.status == QuestStatus.InExecution ||
            questState.status == QuestStatus.InTravelBack;

        // 1) блокер
        if (inProgressBlocker != null)
            inProgressBlocker.SetActive(isInProgress);

        // 2) если квест в процессе — восстанавливаем авантюристов по слотам
        if (isInProgress)
        {
            RestoreAssignedAdventurers(questState);
        }
    }
    
    public void OnClick_OpenSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 5) return;

        // строго по очереди: доступен только следующий слот
        if (slotIndex != selectedAdventurerIds.Count)
            return;

        if (CabinetUIController.Instance == null)
            return;

        CabinetUIController.Instance.OpenAdventurerCarousel(this, slotIndex);
    }

    /// <summary>
    /// Вызывается каруселью после выбора.
    /// </summary>
    public void OnAdventurerChosen(int slotIndex, string adventurerId)
    {
        if (string.IsNullOrEmpty(adventurerId)) return;
        if (slotIndex < 0 || slotIndex >= 5) return;

        // защита от дубля
        if (selectedAdventurerIds.Contains(adventurerId))
            return;

        // слот должен быть следующий по порядку
        if (slotIndex != selectedAdventurerIds.Count)
            return;

        selectedAdventurerIds.Add(adventurerId);
        SpawnSelectedCard(slotIndex, adventurerId);

        UpdateSlotButtonsState();
    }
    
    private void RestoreAssignedAdventurers(QuestStateDTO questState)
    {
        // отключаем интерактив слотов и кнопки отправки
        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (slotButtons[i] != null)
                slotButtons[i].interactable = false;
        }

        if (sendButton != null)
            sendButton.interactable = false;

        // создаём карты в слотах по assignedAdventurer1-5
        TrySpawnAssigned(0, questState.assignedAdventurer1);
        TrySpawnAssigned(1, questState.assignedAdventurer2);
        TrySpawnAssigned(2, questState.assignedAdventurer3);
        TrySpawnAssigned(3, questState.assignedAdventurer4);
        TrySpawnAssigned(4, questState.assignedAdventurer5);
    }

    private void TrySpawnAssigned(int slotIndex, string adventurerId)
    {
        if (string.IsNullOrEmpty(adventurerId)) return;
        if (slotIndex < 0 || slotIndex >= slotCardParents.Length) return;

        // просто создаём карту (без изменения статусов)
        SpawnSelectedCard(slotIndex, adventurerId);
    }

    private void SpawnSelectedCard(int slotIndex, string adventurerId)
    {
        var parent = slotCardParents[slotIndex];
        if (parent == null) return;

        foreach (Transform child in parent)
            Destroy(child.gameObject);

        var prefabs = CabinetUIController.Instance != null
            ? CabinetUIController.Instance.GetAdventurerCardPrefabs()
            : null;

        if (prefabs == null || prefabs.Length == 0) return;

        var prefab = prefabs.FirstOrDefault(p => p != null && p.AdventurerId == adventurerId);
        if (prefab == null) return;

        var go = Instantiate(prefab.gameObject, parent);

        // обновим статы
        var data = GameRepository.Data;
        if (data != null && data.adventurers != null)
        {
            var dto = data.adventurers.FirstOrDefault(a => a.id == adventurerId);
            var card = go.GetComponent<AdventurerCard>();
            if (dto != null && card != null)
                card.RefreshFromData(dto);
        }
    }

    private void UpdateSlotButtonsState()
    {
        // 0..4: активен только "следующий" слот
        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (slotButtons[i] == null) continue;
            slotButtons[i].interactable = (i == selectedAdventurerIds.Count) && (i < 5);
        }

        if (sendButton != null)
            sendButton.interactable = selectedAdventurerIds.Count > 0;
    }

    public void OnClick_SendQuest()
    {
        var data = GameRepository.Data;
        if (data == null) return;

        var questState = QuestService.GetState(data, questId);
        if (questState == null) return;

        // 1) квест в путь
        questState.status = QuestStatus.InTravelTo;

        // 2) записываем 5 слотов
        questState.assignedAdventurer1 = selectedAdventurerIds.Count > 0 ? selectedAdventurerIds[0] : null;
        questState.assignedAdventurer2 = selectedAdventurerIds.Count > 1 ? selectedAdventurerIds[1] : null;
        questState.assignedAdventurer3 = selectedAdventurerIds.Count > 2 ? selectedAdventurerIds[2] : null;
        questState.assignedAdventurer4 = selectedAdventurerIds.Count > 3 ? selectedAdventurerIds[3] : null;
        questState.assignedAdventurer5 = selectedAdventurerIds.Count > 4 ? selectedAdventurerIds[4] : null;

        // 3) авантюристы -> OnQuest
        foreach (var advId in selectedAdventurerIds)
        {
            var adv = data.adventurers.FirstOrDefault(a => a.id == advId);
            if (adv != null)
                adv.status = AdventurerStatus.OnQuest;
        }
        
        QuestPathsManager.Instance.ActivatePath(questId);

        GameRepository.Save();
        
        inProgressBlocker.SetActive(true);
    }

    public void ResetAndReleaseSelectedAdventurers()
    {
        var data = GameRepository.Data;
        if (data != null && data.adventurers != null)
        {
            foreach (var id in selectedAdventurerIds)
            {
                var adv = data.adventurers.FirstOrDefault(a => a.id == id);
                if (adv != null && adv.status == AdventurerStatus.Selected)
                    adv.status = AdventurerStatus.Available;
            }

            GameRepository.Save();
        }

        ResetSelectionVisualOnly();
        UpdateSlotButtonsState();
    }

    private void ResetSelectionVisualOnly()
    {
        selectedAdventurerIds.Clear();

        // чистим спавненные карты под слотами
        for (int i = 0; i < slotCardParents.Length; i++)
        {
            if (slotCardParents[i] == null) continue;
            foreach (Transform child in slotCardParents[i])
                Destroy(child.gameObject);
        }
    }
}
