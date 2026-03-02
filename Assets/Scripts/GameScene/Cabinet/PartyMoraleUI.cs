using UnityEngine;
using UnityEngine.UI;

public class PartyMoraleUI : MonoBehaviour
{
    [Header("Sliders (0..1)")]
    [SerializeField] private Slider mainSlider;         // SliderMoraleMain
    [SerializeField] private Slider previewPlusSlider;  // SliderMoralePreviewPlus (зелёный)
    [SerializeField] private Slider previewMinusSlider; // SliderMoralePreviewMinus (красный)

    private float _mainValue01; // “реальный main” (по выбранным)

    public void SetMainPercent(int percent0_100)
    {
        _mainValue01 = Mathf.Clamp01(percent0_100 / 100f);

        // когда обновили main — сбрасываем превью
        mainSlider.value = _mainValue01;
        if (previewPlusSlider != null) previewPlusSlider.value = 0f;
        if (previewMinusSlider != null) previewMinusSlider.value = 0f;
    }

    public void SetPreviewPercent(int preview0_100)
    {
        float preview01 = Mathf.Clamp01(preview0_100 / 100f);

        // reset
        if (previewPlusSlider != null) previewPlusSlider.value = 0f;
        if (previewMinusSlider != null) previewMinusSlider.value = 0f;

        if (preview01 >= _mainValue01)
        {
            // рост: main остаётся на базовом, зелёный показывает до preview
            mainSlider.value = _mainValue01;
            if (previewPlusSlider != null) previewPlusSlider.value = preview01;
        }
        else
        {
            // падение: main временно опускаем до preview,
            // а красный держим на старом main (чтобы красный был "под main")
            mainSlider.value = preview01;
            if (previewMinusSlider != null) previewMinusSlider.value = _mainValue01;
        }
    }

    public void ClearPreview()
    {
        mainSlider.value = _mainValue01;
        if (previewPlusSlider != null) previewPlusSlider.value = 0f;
        if (previewMinusSlider != null) previewMinusSlider.value = 0f;
    }
}