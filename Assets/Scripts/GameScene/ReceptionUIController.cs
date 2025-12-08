using System.Linq;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.UI;

public class ReceptionUIController : MonoBehaviour
{
    //[SerializeField] private Image currentVisitorImage;

    public void OnClick_ShowCurrentVisitor()
    {
        var vm = VisitorManager.Instance;
        if (vm == null) return;

        var state = vm.GetFirstTodayVisitor();
        if (state == null)
        {
            Debug.Log("[ReceptionUI] No visitors today.");
            return;
        }

        var visitorDef = VisitorService.GetDefinition(state.id);
        if (visitorDef == null)
        {
            Debug.LogWarning("[ReceptionUI] No VisitorDefinition for " + state.id);
            return;
        }

       // if (currentVisitorImage != null)
           // currentVisitorImage.sprite = visitorDef.portrait;

        // если у визитора нет квеста (торговец / misc) — просто портрет
        if (string.IsNullOrEmpty(state.queuedQuestId))
        {
            Debug.Log("[ReceptionUI] Visitor has no queued quest, only portrait shown.");
            return;
        }

        var questDef = QuestService.GetDef(state.queuedQuestId);
        if (questDef == null)
        {
            Debug.LogWarning("[ReceptionUI] No QuestDefinition for " + state.queuedQuestId);
            return;
        }

        if (questDef.dialogueTitles == null || questDef.dialogueTitles.Length == 0)
        {
            Debug.LogWarning("[ReceptionUI] Quest " + questDef.id + " has no dialogueTitles.");
            return;
        }

        var convo = questDef.dialogueTitles;

        Debug.Log($"[ReceptionUI] Start quest dialogue '{convo}' (visitor: {state.id}, quest: {questDef.id})");

        DialogueManager.StartConversation(convo);
    }
}