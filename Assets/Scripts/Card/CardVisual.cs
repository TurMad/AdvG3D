using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class CardVisual : MonoBehaviour
{
    private bool initialized = false;

    [Header("Card")]
    public Card parentCard;
    private RectTransform parentCardRect;
    private RectTransform visualRect;
    private RectTransform visualParentRect;
    private Vector2 rotationDelta;
    private Vector2 movementDelta;
    private int savedIndex;
    private Canvas canvas;

    [Header("References")]
    public Transform visualShadow;
    private float shadowOffset = 20f;
    private Vector2 shadowDistance;
    private Canvas shadowCanvas;
    [SerializeField] private Transform shakeParent;
    [SerializeField] private Transform tiltParent;
    [SerializeField] private Image cardImage;

    [Header("Follow Parameters")]
    [SerializeField] private float followSpeed = 30f;

    [Header("Rotation Parameters")]
    [SerializeField] private float rotationAmount = 20f;
    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] private float manualTiltAmount = 0.05f;
    [SerializeField] private float tiltSpeed = 20f;

    [Header("Scale Parameters")]
    [SerializeField] private bool scaleAnimations = true;
    [SerializeField] private float scaleOnHover = 1.15f;
    [SerializeField] private float scaleOnSelect = 1.25f;
    [SerializeField] private float scaleTransition = .15f;
    [SerializeField] private Ease scaleEase = Ease.OutBack;

    [Header("Select Parameters")]
    [SerializeField] private float selectPunchAmount = 20f;

    [Header("Hover Parameters")]
    [SerializeField] private float hoverPunchAngle = 5f;
    [SerializeField] private float hoverTransition = .15f;

    [Header("Swap Parameters")]
    [SerializeField] private bool swapAnimations = true;
    [SerializeField] private float swapRotationAngle = 30f;
    [SerializeField] private float swapTransition = .15f;
    [SerializeField] private int swapVibrato = 5;

    [Header("Curve")]
    [SerializeField] private CurveParameters curve;

    private float curveYOffset;
    private float curveRotationOffset;

    private Camera uiCamera;

    
    public void Initialize(Card target, int index = 0)
    {
        parentCard = target;
        parentCardRect = target != null ? target.GetComponent<RectTransform>() : null;
        visualRect = GetComponent<RectTransform>();
        visualParentRect = transform.parent as RectTransform;
        canvas = GetComponentInParent<Canvas>();

        uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        if (visualShadow != null)
            shadowCanvas = visualShadow.GetComponent<Canvas>();

        if (parentCard == null || parentCardRect == null || visualRect == null || visualParentRect == null)
            return;

        parentCard.PointerEnterEvent.AddListener(PointerEnter);
        parentCard.PointerExitEvent.AddListener(PointerExit);
        parentCard.BeginDragEvent.AddListener(BeginDrag);
        parentCard.EndDragEvent.AddListener(EndDrag);
        parentCard.PointerDownEvent.AddListener(PointerDown);
        parentCard.PointerUpEvent.AddListener(PointerUp);
        parentCard.SelectEvent.AddListener(Select);

        initialized = true;
    }

    public void UpdateIndex(int length)
    {
        if (parentCard != null && parentCard.transform.parent != null)
            transform.SetSiblingIndex(parentCard.transform.parent.GetSiblingIndex());
    }

    private void Update()
    {
        if (!initialized || parentCard == null || parentCardRect == null || visualRect == null || visualParentRect == null)
            return;

        HandPositioning();
        SmoothFollow();
        FollowRotation();
        CardTilt();
    }

    private void HandPositioning()
    {
        curveYOffset = 0f;
        curveRotationOffset = 0f;
    }

    private void SmoothFollow()
    {
        Vector2 targetPosition = GetCardPositionInVisualParentSpace();

        visualRect.anchoredPosition = Vector2.Lerp(
            visualRect.anchoredPosition,
            targetPosition,
            followSpeed * Time.deltaTime
        );
    }

    private void FollowRotation()
    {
        if (!parentCard.isDragging)
        {
            rotationDelta = Vector2.Lerp(rotationDelta, Vector2.zero, rotationSpeed * Time.deltaTime);

            Vector3 currentAngles = visualRect.localEulerAngles;
            float z = Mathf.LerpAngle(currentAngles.z, 0f, rotationSpeed * Time.deltaTime);
            visualRect.localEulerAngles = new Vector3(currentAngles.x, currentAngles.y, z);
            return;
        }

        Vector2 targetPosition = GetCardPositionInVisualParentSpace();
        Vector2 movement = visualRect.anchoredPosition - targetPosition;

        movementDelta = Vector2.Lerp(movementDelta, movement, 25f * Time.deltaTime);
        rotationDelta = Vector2.Lerp(rotationDelta, movementDelta * rotationAmount, rotationSpeed * Time.deltaTime);

        Vector3 angles = visualRect.localEulerAngles;
        visualRect.localEulerAngles = new Vector3(angles.x, angles.y, Mathf.Clamp(rotationDelta.x, -20f, 20f));
    }

    private void CardTilt()
    {
        Vector2 targetPosition = GetCardPositionInVisualParentSpace();

        visualRect.anchoredPosition = Vector2.Lerp(
            visualRect.anchoredPosition,
            targetPosition,
            followSpeed * Time.deltaTime
        );
    }

    private Vector2 GetCardPositionInVisualParentSpace()
    {
        Vector2 targetPosition = Vector2.zero;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, parentCardRect.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(visualParentRect, screenPoint, uiCamera, out targetPosition);

        return targetPosition;
    }

    private void Select(Card card, bool state)
    {
        DOTween.Kill(2, true);

        float dir = state ? 1f : 0f;
        shakeParent.DOPunchPosition(shakeParent.up * selectPunchAmount * dir, scaleTransition, 10, 1);

        if (!scaleAnimations)
            return;

        float targetScale;

        if (state)
            targetScale = scaleOnSelect;
        else
            targetScale = parentCard.isHovering ? scaleOnHover : 1f;

        transform.DOScale(targetScale, scaleTransition).SetEase(scaleEase);
    }

    public void Swap(float dir = 1f)
    {
        if (!swapAnimations)
            return;

        DOTween.Kill(3, true);
    }

    private void BeginDrag(Card card)
    {
        if (scaleAnimations)
            transform.DOScale(scaleOnSelect, scaleTransition).SetEase(scaleEase);

        if (canvas != null)
            canvas.overrideSorting = true;
    }

    private void EndDrag(Card card)
    {
        if (canvas != null)
            canvas.overrideSorting = false;

        if (!scaleAnimations)
            return;

        float targetScale;

        if (parentCard.selected)
            targetScale = scaleOnSelect;
        else if (parentCard.isHovering)
            targetScale = scaleOnHover;
        else
            targetScale = 1f;

        transform.DOScale(targetScale, scaleTransition).SetEase(scaleEase);
    }

    private void PointerEnter(Card card)
    {
        if (scaleAnimations)
            transform.DOScale(scaleOnHover, scaleTransition).SetEase(scaleEase);

        DOTween.Kill(2, true);
    }

    private void PointerExit(Card card)
    {
        if (parentCard.wasDragged)
            return;

        float targetScale = parentCard.selected ? scaleOnSelect : 1f;
        transform.DOScale(targetScale, scaleTransition).SetEase(scaleEase);
    }

    private void PointerUp(Card card, bool longPress)
    {
        if (scaleAnimations)
            transform.DOScale(longPress ? scaleOnHover : scaleOnSelect, scaleTransition).SetEase(scaleEase);

        if (canvas != null)
            canvas.overrideSorting = false;

        if (visualShadow != null)
            ((RectTransform)visualShadow).anchoredPosition = shadowDistance;

    }

    private void PointerDown(Card card)
    {
        if (scaleAnimations)
            transform.DOScale(scaleOnSelect, scaleTransition).SetEase(scaleEase);

        if (visualShadow != null)
            ((RectTransform)visualShadow).anchoredPosition += Vector2.down * shadowOffset;

    }
}