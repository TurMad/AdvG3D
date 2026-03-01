using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class QuestSendWindow : MonoBehaviour
{
    [Tooltip("ID квеста, для которого это окно отправки.")]
    public string questId;

    [Header("Slots UI")]
    [SerializeField] private Button[] slotButtons;        
    [SerializeField] private Transform[] slotCardParents;
    
    [Header("Sliders")]
    [SerializeField] private PartyBalanceUI balanceUI;

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
        balanceUI.Init(questState.questRank);
        if (questState == null) return;

        bool isInProgress =
            questState.status == QuestStatus.InTravelTo ||
            questState.status == QuestStatus.InExecution ||
            questState.status == QuestStatus.InTravelBack;

        if (inProgressBlocker != null)
            inProgressBlocker.SetActive(isInProgress);

        if (isInProgress)
        {
            RestoreAssignedAdventurers(questState);
            UpdateBalanceFromAssigned(questState);
        }
    }

    private void UpdateBalanceFromAssigned(QuestStateDTO questState)
    {
        if (balanceUI == null) return;

        var data = GameRepository.Data;
        if (data == null || data.adventurers == null) return;

        var selected = new List<AdventurerDTO>();

        AddIfExists(selected, questState.assignedAdventurer1);
        AddIfExists(selected, questState.assignedAdventurer2);
        AddIfExists(selected, questState.assignedAdventurer3);
        AddIfExists(selected, questState.assignedAdventurer4);
        AddIfExists(selected, questState.assignedAdventurer5);

        balanceUI.UpdateMain(selected);

        void AddIfExists(List<AdventurerDTO> list, string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            var adv = data.adventurers.FirstOrDefault(a => a != null && a.id == id);
            if (adv != null) list.Add(adv);
        }
    }

    public void OnClick_OpenSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 5) return;

        // строго по очереди
        if (slotIndex != selectedAdventurerIds.Count)
            return;

        if (QuestSendUIController.Instance == null)
            return;

        QuestSendUIController.Instance.OpenAdventurerCarousel(this, slotIndex);
    }

    public void OnAdventurerChosen(int slotIndex, string adventurerId)
    {
        if (string.IsNullOrEmpty(adventurerId)) return;
        if (slotIndex < 0 || slotIndex >= 5) return;

        if (selectedAdventurerIds.Contains(adventurerId))
            return;

        if (slotIndex != selectedAdventurerIds.Count)
            return;

        selectedAdventurerIds.Add(adventurerId);
        SpawnSelectedCard(slotIndex, adventurerId);

        UpdateSlotButtonsState();
        UpdateBalanceUI();
    }
    
    private void UpdateBalanceUI()
    {
        if (balanceUI == null) return;

        var data = GameRepository.Data;

        var selectedDtos = selectedAdventurerIds
            .Select(id => data.adventurers.FirstOrDefault(a => a.id == id))
            .Where(a => a != null)
            .ToList();

        balanceUI.UpdateMain(selectedDtos);
    }

    private void RestoreAssignedAdventurers(QuestStateDTO questState)
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (slotButtons[i] != null)
                slotButtons[i].interactable = false;
        }

        if (sendButton != null)
            sendButton.interactable = false;

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

        SpawnSelectedCard(slotIndex, adventurerId);
    }

    private void SpawnSelectedCard(int slotIndex, string adventurerId)
    {
        var parent = slotCardParents[slotIndex];
        if (parent == null) return;

        foreach (Transform child in parent)
            Destroy(child.gameObject);

        var prefabs = QuestSendUIController.Instance != null
            ? QuestSendUIController.Instance.GetAdventurerCardPrefabs()
            : null;

        if (prefabs == null || prefabs.Length == 0) return;

        var prefab = prefabs.FirstOrDefault(p => p != null && p.AdventurerId == adventurerId);
        if (prefab == null) return;

        var go = Instantiate(prefab.gameObject, parent);

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

        questState.status = QuestStatus.InTravelTo;
        questState.travelElapsedSeconds = 0f;
        questState.executeHoursRemaining = 0;

        questState.assignedAdventurer1 = selectedAdventurerIds.Count > 0 ? selectedAdventurerIds[0] : null;
        questState.assignedAdventurer2 = selectedAdventurerIds.Count > 1 ? selectedAdventurerIds[1] : null;
        questState.assignedAdventurer3 = selectedAdventurerIds.Count > 2 ? selectedAdventurerIds[2] : null;
        questState.assignedAdventurer4 = selectedAdventurerIds.Count > 3 ? selectedAdventurerIds[3] : null;
        questState.assignedAdventurer5 = selectedAdventurerIds.Count > 4 ? selectedAdventurerIds[4] : null;

        foreach (var advId in selectedAdventurerIds)
        {
            var adv = data.adventurers.FirstOrDefault(a => a.id == advId);
            if (adv != null)
                adv.status = AdventurerStatus.OnQuest;
        }

        QuestPathsManager.Instance.ActivatePath(questId);

        // скрываем иконку на карте, т.к. квест уже в пути
        if (QuestMapIconsManager.Instance != null)
            QuestMapIconsManager.Instance.HideIcon(questId);

        GameRepository.Save();

        if (inProgressBlocker != null)
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

        for (int i = 0; i < slotCardParents.Length; i++)
        {
            if (slotCardParents[i] == null) continue;
            foreach (Transform child in slotCardParents[i])
                Destroy(child.gameObject);
        }
    }
}