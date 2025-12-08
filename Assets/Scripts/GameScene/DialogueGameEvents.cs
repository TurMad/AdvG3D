using UnityEngine;

public class DialogueGameEvents : MonoBehaviour
{
    public void RemoveFirstVisitorFromToday()
    {
        if (VisitorManager.Instance == null)
            return;

        VisitorManager.Instance.RemoveFirstTodayVisitor();
    }
    
    public void SetVisitorUnavailable(string visitorId)
    {
        if (string.IsNullOrEmpty(visitorId))
            return;

        var data = GameRepository.Data;
        if (data == null || data.visitors == null)
            return;

        var state = data.visitors.Find(v => v.id == visitorId);
        if (state == null)
            return;

        state.status = VisitorStatus.NotAvailable;
        GameRepository.Save();
    }
    
    public void SetQuestReceived(string questId)
    {
        if (string.IsNullOrEmpty(questId))
            return;

        var data = GameRepository.Data;
        if (data == null)
            return;

        var questState = QuestService.GetState(data, questId);
        if (questState == null)
            return;

        questState.status = QuestStatus.Received;
        GameRepository.Save();
    }
}
