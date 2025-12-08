using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class AdventurerCardsPanelController : MonoBehaviour
{
    [Header("Panel Move")]
    [SerializeField] private RectTransform panelRect;     // сама панель (RectTransform)
    [SerializeField] private Vector2 hiddenPosition;      // позиция, когда панель спрятана (справа за экраном)
    [SerializeField] private Vector2 shownPosition;       // позиция, когда панель показана
    [SerializeField] private float moveDuration = 0.4f;

    [Header("Cards")]
    [SerializeField] private Transform cardsParent;       // Content у ScrollView
    [SerializeField] private AdventurerCard[] cardPrefabs; // УНИКАЛЬНЫЕ префабы карт (с уже забитыми ID)

    private readonly List<AdventurerCard> _spawnedCards = new();

    /// <summary>
    /// Вызывается из кнопки "Показать список авантюристов".
    /// Обновляет список карт и выдвигает панель.
    /// </summary>
    public void ShowPanel()
    {
        RebuildCardsList();

        if (panelRect == null)
            return;

        // стартуем из скрытой позиции
        panelRect.anchoredPosition = hiddenPosition;
        panelRect.DOAnchorPos(shownPosition, moveDuration).SetEase(Ease.OutCubic);
    }

    /// <summary>
    /// Пересобирает список карт на основе префабов и данных.
    /// Показываем только авантюристов со статусом Available.
    /// </summary>
    private void RebuildCardsList()
    {
        ClearCards();

        var data = GameRepository.Data;
        if (data == null || data.adventurers == null)
            return;

        if (cardPrefabs == null || cardPrefabs.Length == 0)
            return;

        foreach (var prefab in cardPrefabs)
        {
            if (prefab == null) continue;

            // ID берём из самого префаба (ты их уже забиваешь руками)
            string id = prefab.AdventurerId;
            if (string.IsNullOrEmpty(id)) continue;

            var dto = data.adventurers.FirstOrDefault(a => a.id == id);
            if (dto == null) continue;

            // Пока показываем только доступных
            if (dto.status != AdventurerStatus.Available)
                continue;

            var go = Instantiate(prefab.gameObject, cardsParent);
            var cardInstance = go.GetComponent<AdventurerCard>();
            if (cardInstance != null)
            {
                // Обновляем визуал (статы, тексты)
                cardInstance.RefreshFromData(dto);
                _spawnedCards.Add(cardInstance);
            }
        }
    }

    private void ClearCards()
    {
        foreach (var card in _spawnedCards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }

        _spawnedCards.Clear();
    }
}
