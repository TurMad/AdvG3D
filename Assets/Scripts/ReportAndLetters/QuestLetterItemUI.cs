using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestLetterItemUI : MonoBehaviour
{
    public string BoundQuestId { get; private set; }

    [Header("UI")]
    [SerializeField] private Image canvasImage;
    [SerializeField] private TMP_Text bodyText;

    [Header("Changes")]
    [SerializeField] private TMP_Text goldChangeText;
    [SerializeField] private TMP_Text reputationChangeText;

    public void Bind(string questId)
    {
        BoundQuestId = questId;

        var data = GameRepository.Data;
        if (data == null) return;

        var letter = data.questLetters?
                         .FirstOrDefault(l => l != null && l.questId == questId && l.status == InboxItemStatus.OnDesk)
                     ?? data.questLetters?.FirstOrDefault(l => l != null && l.questId == questId);

        var def = QuestService.GetDef(questId);
        var questState = QuestService.GetState(data, questId);

        if (canvasImage != null)
            canvasImage.sprite = def != null ? def.letterCanvasSprite : null;

        if (bodyText != null)
        {
            if (def == null || letter == null)
            {
                bodyText.text = "";
            }
            else
            {
                bodyText.text = (letter.result == MissionResult.Success)
                    ? def.successLetterText
                    : def.failLetterText;
            }
        }

        BindChanges(letter, def, questState);
    }

    private void BindChanges(QuestLetterStateDTO letter, QuestDefinition def, QuestStateDTO questState)
    {
        int goldDelta = 0;

        if (letter != null && letter.result == MissionResult.Success && questState != null)
            goldDelta = questState.guildGold;

        bool showGold = goldDelta != 0;
        
        goldChangeText.gameObject.SetActive(showGold);
        goldChangeText.text = FormatSignedValue(goldDelta);

        int reputationDelta = 0;

        if (letter != null && def != null)
        {
            int absReputation = Mathf.Abs(def.reputationChange);

            if (letter.result == MissionResult.Success)
                reputationDelta = absReputation;
            else if (letter.result == MissionResult.Fail)
                reputationDelta = -absReputation;
        }

        bool showReputation = reputationDelta != 0;

        reputationChangeText.gameObject.SetActive(showReputation);
        reputationChangeText.text = FormatSignedValue(reputationDelta);
    }

    private string FormatSignedValue(int value)
    {
        if (value > 0)
            return $"+{value}";

        return value.ToString();
    }
}