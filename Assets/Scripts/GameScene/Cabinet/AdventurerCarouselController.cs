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
    [SerializeField] private Transform centerAnchor;
    [SerializeField] private Transform leftAnchor;
    [SerializeField] private Transform rightAnchor;
    [SerializeField] private Transform poolParent;

    private readonly List<AdventurerCard> _cards = new();
    private int _currentIndex = 0;

    private QuestSendWindow _activeWindow;
    private int _activeSlotIndex = -1;

    private Tween _moveTween;

    private void Awake()
    {
        if (panelRect == null)
            panelRect = GetComponent<RectTransform>();

        // стартуем спрятанными (если объект активен в сцене)
        if (panelRect != null)
            panelRect.anchoredPosition = hiddenPosition;
    }

    /// <summary>
    /// Показывает карусель для конкретного окна и конкретного слота.
    /// Вызывается из CabinetUIController.
    /// </summary>
    public void Show(QuestSendWindow window, int slotIndex)
    {
        _activeWindow = window;
        _activeSlotIndex = slotIndex;

        gameObject.SetActive(true);

        BuildCards();
        UpdateVisibleCards();

        AnimateTo(shownPosition, Ease.OutBack);
    }

    /// <summary>
    /// Скрывает карусель с анимацией и выключает объект после завершения.
    /// </summary>
    public void Hide()
    {
        AnimateTo(hiddenPosition, Ease.InBack, () =>
        {
            ClearCards();
            _activeWindow = null;
            _activeSlotIndex = -1;

            gameObject.SetActive(false);
        });
    }

    public void OnClick_SelectCenter()
    {
        if (_cards.Count == 0) return;

        var selectedCard = _cards[_currentIndex];
        if (selectedCard == null) return;

        string id = selectedCard.AdventurerId;
        if (string.IsNullOrEmpty(id)) return;

        var data = GameRepository.Data;
        if (data != null && data.adventurers != null)
        {
            var dto = data.adventurers.FirstOrDefault(a => a.id == id);
            if (dto != null)
            {
                dto.status = AdventurerStatus.Selected;
                GameRepository.Save();
            }
        }

        // сообщаем окну
        if (_activeWindow != null && _activeSlotIndex >= 0)
            _activeWindow.OnAdventurerChosen(_activeSlotIndex, id);

        Hide();
    }

    public void OnClickNext()
    {
        if (_cards.Count == 0) return;

        _currentIndex = (_currentIndex + 1) % _cards.Count;
        UpdateVisibleCards();
    }

    public void OnClickPrevious()
    {
        if (_cards.Count == 0) return;

        _currentIndex = (_currentIndex - 1 + _cards.Count) % _cards.Count;
        UpdateVisibleCards();
    }

    private void BuildCards()
    {
        ClearCards();

        var data = GameRepository.Data;
        if (data == null || data.adventurers == null)
            return;

        var prefabs = CabinetUIController.Instance != null
            ? CabinetUIController.Instance.GetAdventurerCardPrefabs()
            : null;

        if (prefabs == null || prefabs.Length == 0)
            return;

        foreach (var prefab in prefabs)
        {
            if (prefab == null) continue;

            string id = prefab.AdventurerId;
            if (string.IsNullOrEmpty(id)) continue;

            var dto = data.adventurers.FirstOrDefault(a => a.id == id);
            if (dto == null) continue;

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
        for (int i = 0; i < _cards.Count; i++)
        {
            if (_cards[i] != null)
                Destroy(_cards[i].gameObject);
        }

        _cards.Clear();
        _currentIndex = 0;
    }

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
                SetCardToAnchor(card, centerAnchor);
                card.gameObject.SetActive(true);
            }
            else if (count == 1)
            {
                HideToPool(card);
            }
            else if (count == 2)
            {
                // при двух: центр + правый (второй)
                int other = (center == 0) ? 1 : 0;

                if (i == other)
                {
                    SetCardToAnchor(card, rightAnchor);
                    card.gameObject.SetActive(true);
                }
                else
                {
                    HideToPool(card);
                }
            }
            else // 3+
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
                    HideToPool(card);
                }
            }
        }
    }

    private void HideToPool(AdventurerCard card)
    {
        card.gameObject.SetActive(false);
        if (poolParent != null)
            card.transform.SetParent(poolParent, false);
    }

    private void SetCardToAnchor(AdventurerCard card, Transform anchor)
    {
        if (anchor == null) return;

        card.transform.SetParent(anchor, false);
        card.transform.localPosition = Vector3.zero;
        card.transform.localRotation = Quaternion.identity;
        card.transform.localScale = Vector3.one;
    }

    private void AnimateTo(Vector2 target, Ease ease, System.Action onComplete = null)
    {
        if (panelRect == null)
            panelRect = GetComponent<RectTransform>();

        if (panelRect == null)
        {
            onComplete?.Invoke();
            return;
        }

        _moveTween?.Kill();
        panelRect.DOKill();

        _moveTween = panelRect
            .DOAnchorPos(target, moveDuration)
            .SetEase(ease)
            .OnComplete(() => onComplete?.Invoke());
    }
}
