using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionReportItemUI : MonoBehaviour
{
    public string BoundReportId { get; private set; }
    [Header("Text")]
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text partyPowerBaseText;
    [SerializeField] private TMP_Text partyBalanceMultiplierText;
    [SerializeField] private TMP_Text partyMoraleMultiplierText;
    [SerializeField] private TMP_Text partyPowerFinalText;
    
    [Header("Adventurers (5 slots)")]
    [SerializeField] private Image[] adventurerImages;     // size = 5
    [SerializeField] private Slider[] xpSliders;           // size = 5
    [SerializeField] private TMP_Text[] xpAddedTexts; 

    public void Bind(MissionReportDTO report)
    {
        if (report == null) return;

        BoundReportId = report.reportId;
        if (resultText)
        {
            // result: 1 Success, 2 Fail (у тебя enum)
            resultText.text = report.result == MissionResult.Success ? "SUCCESS"
                : report.result == MissionResult.Fail ? "FAIL"
                : "NONE";
        }

        if (partyPowerBaseText)
            partyPowerBaseText.text = report.partyPowerBase.ToString();
        
        if (partyBalanceMultiplierText)
            partyBalanceMultiplierText.text = $"x{report.balanceMultiplier:0.00}";
        
        if (partyMoraleMultiplierText)
            partyMoraleMultiplierText.text = $"x{report.moraleMultiplier:0.00}";

        if (partyPowerFinalText)
            partyPowerFinalText.text = report.partyPowerFinal.ToString();
        
        
        BindAdventurers(report);
    }
    
    private void BindAdventurers(MissionReportDTO report)
    {
        // safety
        int slots = 5;

        for (int i = 0; i < slots; i++)
        {
            if (adventurerImages != null && i < adventurerImages.Length && adventurerImages[i] != null)
            {
                adventurerImages[i].sprite = null;
                adventurerImages[i].enabled = false;
            }

            if (xpSliders != null && i < xpSliders.Length && xpSliders[i] != null)
            {
                xpSliders[i].minValue = 0;
                xpSliders[i].maxValue = 1;
                xpSliders[i].value = 0;
            }

            if (xpAddedTexts != null && i < xpAddedTexts.Length && xpAddedTexts[i] != null)
                xpAddedTexts[i].text = "";
        }

        var data = GameRepository.Data;
        if (data == null) return;

        for (int i = 0; i < slots && i < report.adventurerIds.Count; i++)
        {
            var id = report.adventurerIds[i];
            if (string.IsNullOrEmpty(id)) continue;

            var adv = data.adventurers.FirstOrDefault(a => a != null && a.id == id);
            if (adv == null) continue;

            var def = AdventurerService.GetDefinition(id);
            var portrait = def != null ? def.portrait : null;

            if (adventurerImages != null && i < adventurerImages.Length && adventurerImages[i] != null)
            {
                adventurerImages[i].sprite = portrait;
                adventurerImages[i].enabled = portrait != null;
            }

            if (xpSliders != null && i < xpSliders.Length && xpSliders[i] != null)
            {
                int max = Mathf.Max(1, adv.xpToNext);
                xpSliders[i].minValue = 0;
                xpSliders[i].maxValue = max;
                xpSliders[i].value = Mathf.Clamp(adv.currentXp, 0, max);
            }

            if (xpAddedTexts != null && i < xpAddedTexts.Length && xpAddedTexts[i] != null)
                xpAddedTexts[i].text = $"+{report.expPerAdventurer} exp";
        }
    }
}