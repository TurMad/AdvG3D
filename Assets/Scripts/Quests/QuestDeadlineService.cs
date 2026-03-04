using System.Linq;

public static class QuestDeadlineService
{
    public static void ProcessNewDay()
    {
        var data = GameRepository.Data;
        if (data == null || data.quests == null) return;

        foreach (var qs in data.quests)
        {
            if (qs == null) continue;

            if (qs.status == QuestStatus.NotReceived ||
                qs.status == QuestStatus.InQueue ||
                qs.status == QuestStatus.Completed ||
                qs.status == QuestStatus.Declined)
                continue;

            if (qs.deadlineDaysRemaining > 0)
                qs.deadlineDaysRemaining--;

            if (qs.deadlineDaysRemaining > 0)
                continue;

            HandleDeadlineExpired(qs);
        }

        GameRepository.Save();
    }

    static void HandleDeadlineExpired(QuestStateDTO qs)
    {
        var data = GameRepository.Data;
        if (data == null) return;

        // ===== сценарий 1 =====
        if (qs.status == QuestStatus.Received)
        {
            qs.status = QuestStatus.Completed;

            if (QuestPathsManager.Instance != null)
                QuestPathsManager.Instance.DeactivatePath(qs.id);

            CreateFailLetter(qs.id);
            QuestMapIconsManager.Instance.HideIcon(qs.id);
            return;
        }

        // ===== сценарий 2 =====
        if (qs.status == QuestStatus.InTravelTo)
        {
            qs.status = QuestStatus.InTravelBack;

            if (QuestPathsManager.Instance != null)
                QuestPathsManager.Instance.ResumePath(qs.id);

            CreateFailLetter(qs.id);
            return;
        }

        // ===== сценарий 3 =====
        // InExecution или InTravelBack — ничего не делаем
    }

    static void CreateFailLetter(string questId)
    {
        var data = GameRepository.Data;
        if (data == null) return;

        if (data.questLetters == null)
            data.questLetters = new System.Collections.Generic.List<QuestLetterStateDTO>();

        var letter = data.questLetters.FirstOrDefault(l => l != null && l.questId == questId);

        if (letter == null)
        {
            letter = new QuestLetterStateDTO();
            data.questLetters.Add(letter);
        }

        letter.questId = questId;
        letter.status = InboxItemStatus.OnDesk;
        letter.hoursRemaining = 0;
        letter.result = MissionResult.Fail;
    }
}