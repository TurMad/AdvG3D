using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdventurerCard : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private string adventurerId;
    public string AdventurerId
    {
        get => adventurerId;
        set => adventurerId = value;
    }

    [Header("Front Visual")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private TMP_Text powerText;

    public void RefreshFromData(AdventurerDTO dto)
    {
        if (dto == null) return;

        var def = AdventurerService.GetDefinition(adventurerId);
        if (def != null && portraitImage != null)
            portraitImage.sprite = def.portrait;

        var rank = RankUtils.GetRankByLevel(dto.level);

        if (rankText != null)
            rankText.text = rank.ToString();

        if (energyText != null)
            energyText.text = dto.energy.ToString();

        if (powerText != null)
            powerText.text = CombatPowerCalculator.GetVisiblePower(dto).ToString();
    }
}