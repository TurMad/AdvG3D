using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private Button add50Button;

    private void Start()
    {
        GameRepository.InitOrLoad(); // грузим или создаем
        RefreshGoldUI();
        if (add50Button != null) add50Button.onClick.AddListener(OnAdd50);
    }

    private void OnAdd50()
    {
        GameRepository.Data.gold += 50;
        RefreshGoldUI();
        GameRepository.Save(); // сразу сохраняем
    }

    private void RefreshGoldUI()
    {
        if (goldText != null) goldText.text = GameRepository.Data.gold.ToString();
    }
}