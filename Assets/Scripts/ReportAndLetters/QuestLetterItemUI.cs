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

    public void Bind(string questId)
    {
        BoundQuestId = questId;

        var data = GameRepository.Data;
        if (data == null) return;

        // 1) берём dto письма (лучше сначала то, что на столе)
        var letter = data.questLetters?
                         .FirstOrDefault(l => l != null && l.questId == questId && l.status == InboxItemStatus.OnDesk)
                     ?? data.questLetters?.FirstOrDefault(l => l != null && l.questId == questId);

        // 2) берём деф квеста
        var def = QuestService.GetDef(questId);

        // sprite холста письма
        if (canvasImage != null)
            canvasImage.sprite = def != null ? def.letterCanvasSprite : null;

        // текст письма по результату
        if (bodyText != null)
        {
            if (def == null || letter == null)
            {
                bodyText.text = "";
                return;
            }

            bodyText.text = (letter.result == MissionResult.Success)
                ? def.successLetterText
                : def.failLetterText;
        }
    }
}