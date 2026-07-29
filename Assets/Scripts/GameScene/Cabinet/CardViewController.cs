using DG.Tweening;
using UnityEngine;

public class CardViewController : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private RectTransform rootRect;

    [Header("Child Panels")]
    [SerializeField] private RectTransform scrollViewRect;
    [SerializeField] private RectTransform infoPanelRect;

    [Header("Scroll View Move")]
    [SerializeField] private Vector2 scrollHiddenPosition;
    [SerializeField] private Vector2 scrollShownPosition;

    [Header("Info Panel Move")]
    [SerializeField] private Vector2 infoHiddenPosition;
    [SerializeField] private Vector2 infoShownPosition;

    [Header("Animation")]
    [SerializeField] private float moveDuration = 0.4f;

    private QuestSendWindow _activeWindow;
    private int _activeSlotIndex = -1;

    private Tween _scrollTween;
    private Tween _infoTween;

    private void Awake()
    {
        if (rootRect == null)
            rootRect = GetComponent<RectTransform>();

        if (rootRect != null)
            rootRect.anchoredPosition = Vector2.zero;

        ApplyHiddenStateImmediate();
    }

    public void Show(QuestSendWindow window, int slotIndex)
    {
        _activeWindow = window;
        _activeSlotIndex = slotIndex;

        if (rootRect == null)
            rootRect = GetComponent<RectTransform>();

        if (rootRect != null)
            rootRect.anchoredPosition = Vector2.zero;

        gameObject.SetActive(true);

        ApplyHiddenStateImmediate();
        AnimateChildrenToShown();
    }

    public void Hide()
    {
        AnimateChildrenToHidden(() =>
        {
            _activeWindow?.ClearMoralePreview();
            _activeWindow = null;
            _activeSlotIndex = -1;

            gameObject.SetActive(false);
        });
    }

    public void OnClick_SelectCenter()
    {
    }

    private void ApplyHiddenStateImmediate()
    {
        if (scrollViewRect != null)
            scrollViewRect.anchoredPosition = scrollHiddenPosition;

        if (infoPanelRect != null)
            infoPanelRect.anchoredPosition = infoHiddenPosition;
    }

    private void AnimateChildrenToShown()
    {
        KillTweens();

        if (scrollViewRect != null)
        {
            _scrollTween = scrollViewRect
                .DOAnchorPos(scrollShownPosition, moveDuration)
                .SetEase(Ease.OutBack);
        }

        if (infoPanelRect != null)
        {
            _infoTween = infoPanelRect
                .DOAnchorPos(infoShownPosition, moveDuration)
                .SetEase(Ease.OutBack);
        }
    }

    private void AnimateChildrenToHidden(System.Action onComplete)
    {
        KillTweens();

        int completedCount = 0;
        int targetCount = 0;

        if (scrollViewRect != null)
        {
            targetCount++;
            _scrollTween = scrollViewRect
                .DOAnchorPos(scrollHiddenPosition, moveDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    completedCount++;
                    if (completedCount >= targetCount)
                        onComplete?.Invoke();
                });
        }

        if (infoPanelRect != null)
        {
            targetCount++;
            _infoTween = infoPanelRect
                .DOAnchorPos(infoHiddenPosition, moveDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    completedCount++;
                    if (completedCount >= targetCount)
                        onComplete?.Invoke();
                });
        }

        if (targetCount == 0)
            onComplete?.Invoke();
    }

    private void KillTweens()
    {
        _scrollTween?.Kill();
        _infoTween?.Kill();

        if (scrollViewRect != null)
            scrollViewRect.DOKill();

        if (infoPanelRect != null)
            infoPanelRect.DOKill();
    }
}