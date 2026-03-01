using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestOfferSheetWindow : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image questIconImage;
    [SerializeField] private TMP_Text rankValueText;             
    [SerializeField] private Slider rankSlider;      
    [SerializeField] private Slider guildShareSlider; 
    [SerializeField] private TMP_Text guildGoldText;
    [SerializeField] private TMP_Text adventurersGoldText;

    private string _questId;
    private int _totalGoldForQuest;
    private Rank _selectedRank = Rank.G;

    private void Awake()
    {
        if (rankSlider != null)
        {
            rankSlider.wholeNumbers = true;
            rankSlider.minValue = 0;
            rankSlider.maxValue = 9;
            rankSlider.onValueChanged.AddListener(OnRankSliderChanged);
        }
        if (guildShareSlider != null)
            guildShareSlider.onValueChanged.AddListener(_ => RefreshGoldSplitTexts());
        
        
    }

    public void Open(string questId)
    {
        _questId = questId;
        
        var def = QuestService.GetDef(questId);
        
        if (def != null && questIconImage != null)
        {
            questIconImage.sprite = def.icon;
            questIconImage.enabled = def.icon != null;
        }

        SetRank(Rank.G);
        
        var data = GameRepository.Data;
        var state = QuestService.GetState(data, _questId);
        _totalGoldForQuest = state != null ? state.tradedGold : 0;
        
        
        if (guildShareSlider != null)
        {
            guildShareSlider.minValue = 0;
            guildShareSlider.maxValue = 100;
            guildShareSlider.wholeNumbers = true;
            guildShareSlider.SetValueWithoutNotify(10);
        }

        RefreshGoldSplitTexts();
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
        _questId = null;
    }

    private void OnRankSliderChanged(float value)
    {
        var idx = Mathf.RoundToInt(value);
        idx = Mathf.Clamp(idx, 0, 9);
        SetRank((Rank)idx);
    }
    
    public void OnClick_Accept()
    {
        if (string.IsNullOrEmpty(_questId)) return;

        var data = GameRepository.Data;
        if (data == null) return;

        var state = QuestService.GetState(data, _questId);
        if (state == null) return;
       
        state.status = QuestStatus.Received;
        state.questRank = _selectedRank;
        
        int total = state.tradedGold; // (позже будет после торга)
        int percent = guildShareSlider != null ? Mathf.RoundToInt(guildShareSlider.value) : 10;

        int guild = Mathf.RoundToInt(total * (percent / 100f));
        int adv = total - guild;
        
        state.guildGold = guild;
        state.adventurersGold = adv;
        
        GameRepository.Save();
        if (QuestMapIconsManager.Instance != null)
            QuestMapIconsManager.Instance.ShowIcon(_questId);
        Close();
    }

    private void SetRank(Rank rank)
    {
        _selectedRank = rank;

        if (rankSlider != null)
            rankSlider.SetValueWithoutNotify((int)rank);

        if (rankValueText != null)
            rankValueText.text = rank.ToString();
    }
    
    private void RefreshGoldSplitTexts()
    {
        int percent = guildShareSlider != null ? Mathf.RoundToInt(guildShareSlider.value) : 10;

        int guild = Mathf.RoundToInt(_totalGoldForQuest * (percent / 100f));
        int adv = _totalGoldForQuest - guild;

        if (guildGoldText != null) guildGoldText.text = guild.ToString();
        if (adventurersGoldText != null) adventurersGoldText.text = adv.ToString();
    }
}