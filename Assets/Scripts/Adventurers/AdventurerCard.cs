using System.Collections;
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
    
    [SerializeField] private Image portraitImage;

    [Header("Flip Root & Sides")]
    [SerializeField] private Transform flipRoot;   // пустой объект-контейнер
    [SerializeField] private GameObject frontSide;
    [SerializeField] private GameObject backSide;
    
    [Header("Back Side Stats Texts")]
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private TMP_Text defenseText;
    [SerializeField] private TMP_Text buffText;
    [SerializeField] private TMP_Text debuffText;
    [SerializeField] private TMP_Text healingText;

    [Header("Flip Settings")]
    [SerializeField] private float flipDuration = 0.3f;
    
    [Header("AdditionalUI")]
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private TMP_Text powerText;

    private bool _isFront = true;
    private bool _isFlipping = false;
    private bool _sideSwitched = false;

    private void Awake()
    {
        if (flipRoot == null)
            flipRoot = transform;

        flipRoot.localRotation = Quaternion.identity;

        if (frontSide != null) frontSide.SetActive(true);
        if (backSide != null) backSide.SetActive(false);
    }

    public void OnClickFlip()
    {
        if (_isFlipping) return;
        StartCoroutine(FlipRoutine());
    }

    private IEnumerator FlipRoutine()
    {
        _isFlipping = true;
        _sideSwitched = false;

        float elapsed = 0f;
        Quaternion startRot = flipRoot.localRotation;
        Quaternion targetRot;

        if (_isFront)
            targetRot = Quaternion.Euler(0f, 180f, 0f);
        else
            targetRot = Quaternion.Euler(0f, 0f, 0f);

        while (elapsed < flipDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flipDuration);

            flipRoot.localRotation = Quaternion.Slerp(startRot, targetRot, t);

            if (!_sideSwitched && t >= 0.5f)
            {
                _sideSwitched = true;

                if (frontSide != null) frontSide.SetActive(!_isFront);
                if (backSide != null) backSide.SetActive(_isFront);
            }

            yield return null;
        }

        flipRoot.localRotation = targetRot;
        _isFront = !_isFront;
        _isFlipping = false;
    }
    
    public void RefreshFromData(AdventurerDTO dto)
    {
        var def = AdventurerService.GetDefinition(adventurerId);
        if (attackText  != null) attackText.text  = dto.attack.ToString();
        if (defenseText != null) defenseText.text = dto.defense.ToString();
        if (buffText    != null) buffText.text    = dto.buff.ToString();
        if (debuffText  != null) debuffText.text  = dto.debuff.ToString();
        if (healingText != null) healingText.text = dto.healing.ToString();
        if (portraitImage != null) portraitImage.sprite = def.portrait;
        var rank = RankUtils.GetRankByLevel(dto.level);
        if (rankText != null) rankText.text = rank.ToString();
        if (energyText != null) energyText.text = dto.energy.ToString();
        if (powerText != null) powerText.text = CombatPowerCalculator.GetVisiblePower(dto).ToString();
    }
}
