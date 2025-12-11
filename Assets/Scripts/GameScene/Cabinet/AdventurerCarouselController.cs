using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class AdventurerCarouselController : MonoBehaviour
{
    [Header("Panel Move")]
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private Vector2 hiddenPosition;
    [SerializeField] private Vector2 shownPosition;
    [SerializeField] private float moveDuration = 0.4f;
    
    [Header("Anchors")]
    [SerializeField] private Transform centerAnchor;   // середина
    [SerializeField] private Transform leftAnchor;     // слева
    [SerializeField] private Transform rightAnchor;    // справа
    [SerializeField] private Transform poolParent;     // куда складывать лишние карты (можно пустой объект вне кадра)

    [Header("Prefabs")]
    [SerializeField] private AdventurerCard[] cardPrefabs; // префабы с уже проставленными AdventurerId

    public List<AdventurerCard> _cards = new();
    private int _currentIndex = 0;

    private void OnEnable()
    {
        BuildCards();
        UpdateVisibleCards();
        
        if (panelRect == null)
            panelRect = GetComponent<RectTransform>();

        // запускаем с позиции hidden → к shown, с подпрыгиванием
        panelRect.DOKill();
        panelRect.anchoredPosition = hiddenPosition;
        panelRect
            .DOAnchorPos(shownPosition, moveDuration)
            .SetEase(Ease.OutBack);
    }

    private void OnDisable()
    {
        ClearCards();
        
        if (panelRect == null)
            panelRect = GetComponent<RectTransform>();

        // уезжаем обратно в hidden
        panelRect.DOKill();
        panelRect
            .DOAnchorPos(hiddenPosition, moveDuration)
            .SetEase(Ease.InBack);

        ClearCards();
    }

    /// <summary>
    /// Собираем список доступных авантюристов из префабов.
    /// </summary>
    private void BuildCards()
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

            string id = prefab.AdventurerId;
            if (string.IsNullOrEmpty(id)) continue;

            var dto = data.adventurers.FirstOrDefault(a => a.id == id);
            if (dto == null) continue;

            // Берём только доступных
            if (dto.status != AdventurerStatus.Available)
                continue;

            var go = Instantiate(prefab.gameObject, poolParent);
            var card = go.GetComponent<AdventurerCard>();
            if (card != null)
            {
                card.RefreshFromData(dto);
                _cards.Add(card);
            }
        }

        _currentIndex = 0;
    }

    private void ClearCards()
    {
        foreach (var card in _cards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }

        _cards.Clear();
        _currentIndex = 0;
    }

    /// <summary>
    /// Кнопка "вперёд" (правая стрелка).
    /// </summary>
    public void OnClickNext()
    {
        if (_cards.Count == 0) return;

        _currentIndex = (_currentIndex + 1) % _cards.Count;
        UpdateVisibleCards();
    }

    /// <summary>
    /// Кнопка "назад" (левая стрелка).
    /// </summary>
    public void OnClickPrevious()
    {
        if (_cards.Count == 0) return;

        _currentIndex = (_currentIndex - 1 + _cards.Count) % _cards.Count;
        UpdateVisibleCards();
    }

    /// <summary>
    /// Расставляем карты по трём якорям: центр, лево, право.
    /// Остальные прячем.
    /// </summary>
    private void UpdateVisibleCards()
    {
        if (_cards.Count == 0)
            return;

        int count = _cards.Count;

        int center = _currentIndex;
        int left = (center - 1 + count) % count;
        int right = (center + 1) % count;

        for (int i = 0; i < count; i++)
        {
            var card = _cards[i];
            if (card == null) continue;

            if (i == center)
            {
                // центр всегда показываем
                SetCardToAnchor(card, centerAnchor);
                card.gameObject.SetActive(true);
            }
            else if (count == 1)
            {
                // при одном авантюристе остальные скрыты
                card.gameObject.SetActive(false);
                if (poolParent != null)
                    card.transform.SetParent(poolParent, false);
            }
            else if (count == 2)
            {
                // при двух: только центр и ПРАВЫЙ
                int other = (center == 0) ? 1 : 0;

                if (i == other)
                {
                    SetCardToAnchor(card, rightAnchor);
                    card.gameObject.SetActive(true);
                }
                else
                {
                    card.gameObject.SetActive(false);
                    if (poolParent != null)
                        card.transform.SetParent(poolParent, false);
                }
            }
            else // count >= 3
            {
                if (i == left)
                {
                    SetCardToAnchor(card, leftAnchor);
                    card.gameObject.SetActive(true);
                }
                else if (i == right)
                {
                    SetCardToAnchor(card, rightAnchor);
                    card.gameObject.SetActive(true);
                }
                else
                {
                    card.gameObject.SetActive(false);
                    if (poolParent != null)
                        card.transform.SetParent(poolParent, false);
                }
            }
        }
    }

    private void SetCardToAnchor(AdventurerCard card, Transform anchor)
    {
        if (anchor == null) return;

        card.transform.SetParent(anchor, false);
        card.transform.localPosition = Vector3.zero;
        card.transform.localRotation = Quaternion.identity;
        card.transform.localScale = Vector3.one;
    }
}
