using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestOfferSheetWindow : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image questIconImage;
    [SerializeField] private TMP_Text rankValueText;              // сюда пишем выбранный ранг
    [SerializeField] private TMP_Text goldForAdventurersText;      // пока просто вывод-заглушка
    [SerializeField] private Slider rankSlider;                   // G..SSS

    private string _questId;
    private Rank _selectedRank = Rank.G;

    private void Awake()
    {
        // Слайдер должен быть дискретным
        if (rankSlider != null)
        {
            rankSlider.wholeNumbers = true;
            rankSlider.minValue = 0;
            rankSlider.maxValue = 9;
            rankSlider.onValueChanged.AddListener(OnRankSliderChanged);
        }
    }

    public void Open(string questId)
    {
        _questId = questId;

        // подтягиваем деф
        var def = QuestService.GetDef(questId);
        if (def != null && questIconImage != null)
        {
            questIconImage.sprite = def.icon;
            questIconImage.enabled = def.icon != null;
        }

        // стартовые значения (пока просто G)
        SetRank(Rank.G);

        // золото авантюристам пока не считаем — просто очистим/заглушка
        if (goldForAdventurersText != null)
            goldForAdventurersText.text = "-";

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

        GameRepository.Save();
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
}