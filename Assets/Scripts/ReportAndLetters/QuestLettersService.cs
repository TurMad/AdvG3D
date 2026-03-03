using System.Linq;

public static class QuestLettersService
{
    public static void TickOneHour()
    {
        var data = GameRepository.Data;
        if (data == null || data.questLetters == null) return;

        bool changed = false;

        foreach (var l in data.questLetters)
        {
            if (l == null) continue;
            if (l.status != InboxItemStatus.Pending) continue;

            l.hoursRemaining -= 1;
            if (l.hoursRemaining <= 0)
            {
                l.hoursRemaining = 0;
                l.status = InboxItemStatus.OnDesk;
                changed = true;
            }
        }

        if (changed)
            GameRepository.Save();
    }
}