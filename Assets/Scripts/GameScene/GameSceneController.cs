using UnityEngine;
using TMPro;

public class GameSceneController : MonoBehaviour
{
    public static GameSceneController Instance { get; private set; }

    [Header("Header UI")]
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text repText;

    private void Awake()
    {
        Instance = this;
        RefreshHeader();
    }

    private void Start()
    {
        RefreshHeader();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void RefreshHeader()
    {
        var data = GameRepository.Data;
        if (data == null) return;

        if (goldText != null)
            goldText.text = data.gold.ToString();

        if (repText != null)
            repText.text = data.reputation.ToString();
    }

    public static void ApplyQuestLetterEffects(QuestLetterStateDTO letter)
    {
        var data = GameRepository.Data;
        if (data == null || letter == null) return;

        var def = QuestService.GetDef(letter.questId);
        if (def == null) return;

        var questState = QuestService.GetState(data, letter.questId);

        if (letter.result == MissionResult.Success && questState != null && questState.guildGold != 0)
            data.gold += questState.guildGold;

        int reputationDelta = GetReputationDelta(def.reputationChange, letter.result);
        if (reputationDelta != 0)
            data.reputation += reputationDelta;

        if (Instance != null)
            Instance.RefreshHeader();
    }

    private static int GetReputationDelta(int reputationChange, MissionResult result)
    {
        int absValue = Mathf.Abs(reputationChange);

        if (absValue == 0)
            return 0;

        if (result == MissionResult.Success)
            return absValue;

        if (result == MissionResult.Fail)
            return -absValue;

        return 0;
    }
}