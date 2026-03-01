using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PartyBalanceUI : MonoBehaviour
{
    [SerializeField] private Slider mainSlider;
    [SerializeField] private Slider previewSlider;

    private Rank currentRank;

    public void Init(Rank rank)
    {
        currentRank = rank;
        UpdateMain(new List<AdventurerDTO>());
        previewSlider.value = 0;
    }

    public void UpdateMain(List<AdventurerDTO> selected)
    {
        int percent = PartyBalanceService.CalculateBalancePercent(selected, currentRank);
        mainSlider.value = percent / 100f;
    }

    public void UpdatePreview(
        List<AdventurerDTO> selected,
        AdventurerDTO previewCandidate)
    {
        if (previewCandidate == null)
        {
            previewSlider.value = mainSlider.value;
            return;
        }

        var temp = selected.ToList();
        temp.Add(previewCandidate);

        int previewPercent = PartyBalanceService.CalculateBalancePercent(temp, currentRank);
        previewSlider.value = previewPercent / 100f;
    }
}