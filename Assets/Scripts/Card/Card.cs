using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;

public class Card : MonoBehaviour,
    IDragHandler,
    IBeginDragHandler,
    IEndDragHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerUpHandler,
    IPointerDownHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private RectTransform parentRectTransform;
    private GraphicRaycaster graphicRaycaster;
    private Image imageComponent;
    private CanvasGroup canvasGroup;

    [SerializeField] private bool instantiateVisual = true;
    private VisualCardsHandler visualHandler;

    private Vector2 dragPointerOffset;
    private Camera uiCamera;

    [Header("Selection")]
    public bool selected;
    public float selectionOffset = 50f;
    private float pointerDownTime;
    private float pointerUpTime;

    [Header("Visual")]
    [SerializeField] private GameObject cardVisualPrefab;
    [HideInInspector] public CardVisual cardVisual;

    [Header("States")]
    public bool isHovering;
    public bool isDragging;
    [HideInInspector] public bool wasDragged;

    [Header("Events")]
    [HideInInspector] public UnityEvent<Card> PointerEnterEvent;
    [HideInInspector] public UnityEvent<Card> PointerExitEvent;
    [HideInInspector] public UnityEvent<Card, bool> PointerUpEvent;
    [HideInInspector] public UnityEvent<Card> PointerDownEvent;
    [HideInInspector] public UnityEvent<Card> BeginDragEvent;
    [HideInInspector] public UnityEvent<Card> EndDragEvent;
    [HideInInspector] public UnityEvent<Card, bool> SelectEvent;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        parentRectTransform = transform.parent as RectTransform;
        imageComponent = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
        graphicRaycaster = canvas != null ? canvas.GetComponent<GraphicRaycaster>() : null;

        uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        if (!instantiateVisual)
            return;

        visualHandler = FindObjectOfType<VisualCardsHandler>();
        Transform visualParent = visualHandler != null ? visualHandler.transform : (canvas != null ? canvas.transform : null);

        if (visualParent == null || cardVisualPrefab == null)
            return;

        cardVisual = Instantiate(cardVisualPrefab, visualParent).GetComponent<CardVisual>();
        if (cardVisual != null)
            cardVisual.Initialize(this);
    }

    private void Update()
    {
        if (!isDragging || rectTransform == null || parentRectTransform == null)
            return;

        Vector2 localPointerPosition;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, Input.mousePosition, uiCamera, out localPointerPosition))
            return;

        rectTransform.anchoredPosition = localPointerPosition - dragPointerOffset;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        var holder = GetComponentInParent<HorizontalCardHolder>();
        if (holder != null)
            holder.ForceSelectCard(this);

        BeginDragEvent.Invoke(this);

        if (rectTransform == null)
            return;

        parentRectTransform = transform.parent as RectTransform;

        Vector2 localPointerPosition;
        if (parentRectTransform != null &&
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, eventData.position, eventData.pressEventCamera, out localPointerPosition))
        {
            dragPointerOffset = localPointerPosition - rectTransform.anchoredPosition;
        }
        else
        {
            dragPointerOffset = Vector2.zero;
        }

        isDragging = true;
        wasDragged = true;

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.blocksRaycasts = false;

        if (imageComponent != null)
            imageComponent.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        EndDragEvent.Invoke(this);

        isDragging = false;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        if (imageComponent != null)
            imageComponent.raycastTarget = true;

        var holder = GetComponentInParent<HorizontalCardHolder>();
        if (holder != null)
            holder.ForceSelectCard(this);

        SetSelectedState(true);

        StartCoroutine(FrameWait());
    }

    private IEnumerator FrameWait()
    {
        yield return new WaitForEndOfFrame();
        wasDragged = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnterEvent.Invoke(this);
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExitEvent.Invoke(this);
        isHovering = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        PointerDownEvent.Invoke(this);
        pointerDownTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        pointerUpTime = Time.time;

        bool isLongPress = pointerUpTime - pointerDownTime > .2f;
        PointerUpEvent.Invoke(this, isLongPress);

        if (isLongPress)
            return;

        if (wasDragged)
            return;

        var holder = GetComponentInParent<HorizontalCardHolder>();
        if (holder != null)
        {
            holder.HandleCardSelection(this);
            return;
        }

        SetSelectedState(!selected);
    }
    
    public void SetSelectedState(bool value)
    {
        selected = value;

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (rectTransform != null)
            rectTransform.anchoredPosition = selected ? Vector2.up * selectionOffset : Vector2.zero;

        SelectEvent.Invoke(this, selected);
    }

    public void Deselect()
    {
        if (!selected)
            return;

        SetSelectedState(false);
    }
    
    public void RefreshDragParentAfterSwap()
    {
        rectTransform = rectTransform != null ? rectTransform : GetComponent<RectTransform>();
        parentRectTransform = transform.parent as RectTransform;

        if (!isDragging || rectTransform == null || parentRectTransform == null)
            return;

        Vector2 localPointerPosition;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, Input.mousePosition, uiCamera, out localPointerPosition))
            return;

        dragPointerOffset = localPointerPosition - rectTransform.anchoredPosition;
    }

    public int SiblingAmount()
    {
        return transform.parent != null && transform.parent.CompareTag("Slot")
            ? transform.parent.parent.childCount - 1
            : 0;
    }

    public int ParentIndex()
    {
        return transform.parent != null && transform.parent.CompareTag("Slot")
            ? transform.parent.GetSiblingIndex()
            : 0;
    }

    public float NormalizedPosition()
    {
        return transform.parent != null && transform.parent.CompareTag("Slot")
            ? ExtensionMethods.Remap((float)ParentIndex(), 0, (float)(transform.parent.parent.childCount - 1), 0, 1)
            : 0;
    }

    private void OnDestroy()
    {
        if (cardVisual != null)
            Destroy(cardVisual.gameObject);
    }
}