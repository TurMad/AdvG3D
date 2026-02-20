using TMPro;
using UnityEngine;

public class MissionReportItemUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text partyPowerBaseText;
    [SerializeField] private TMP_Text partyPowerFinalText;

    public void Bind(MissionReportDTO report)
    {
        if (report == null) return;

        if (resultText)
        {
            // result: 1 Success, 2 Fail (у тебя enum)
            resultText.text = report.result == MissionResult.Success ? "SUCCESS"
                : report.result == MissionResult.Fail ? "FAIL"
                : "NONE";
        }

        if (partyPowerBaseText)
            partyPowerBaseText.text = report.partyPowerBase.ToString();

        if (partyPowerFinalText)
            partyPowerFinalText.text = report.partyPowerFinal.ToString();
    }
}