using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapCameraInputController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float MoveSpeed = 20f;
    [SerializeField] AnimationCurve MoveSpeedZoomCurve = AnimationCurve.Linear(0f, 0.5f, 1f, 1f);
    
    [SerializeField] private float Acceleration = 10f;
    [SerializeField] private float Deceleration = 10f;

    [Space(10)] [SerializeField] private float EdgeScrollingMargin = 15f;
    
    private Vector2 edgeScrollInput;
    private Vector3 Velocity = Vector3.zero;
    
    [Header("Orbit")]
    [SerializeField] private float OrbitSensitivity = 0.5f;
    [SerializeField] private float OrbitSoothing = 5f;
    
    [Header("Zoom")]
    [SerializeField] private float ZoomSpeed = 0.5f;
    [SerializeField] private float ZoomSmoothing = 5f;
    
    [Header("Bounds")]
    [SerializeField] private BoxCollider mapBounds;
    [SerializeField] private float targetPadding = 0f;

    private float CurrentZoomSpeed = 0f;

    public float ZoomLevel
    {
        get
        {
            InputAxis axis = OrbitalFollow.RadialAxis;

            return Mathf.InverseLerp(axis.Range.x, axis.Range.y, axis.Value);
        }
    }
    
    [SerializeField] private CinemachineOrbitalFollow OrbitalFollow;
    [SerializeField] private Transform CameraTarget;
    
    #region  Input

    private Vector2 moveInput;
    private Vector2 scrollInput;
    private Vector2 lookInput;
    private bool rightClickInput = false;
    
    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    void OnScrollWheel(InputValue value)
    {
        scrollInput = value.Get<Vector2>(); 
    }
    
    void OnRightClick(InputValue value)
    {
        rightClickInput = value.isPressed; 
    }

    #endregion

    #region UnityMethods

    private void LateUpdate()
    {
        float deltaTime = Time.unscaledDeltaTime;

        if (!Application.isEditor)
        {
            UpdateEdgeScrolling(); 
        }
        UpdateOrbit(deltaTime);
        UpdateMovement(deltaTime);
        UpdateZoom(deltaTime);
    }

    #endregion

    #region Control Methods

    void UpdateMovement(float deltaTime)
    {
        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0f;
        forward.Normalize();
        
        Vector3 right = Camera.main.transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 inputVector = new Vector3(moveInput.x + edgeScrollInput.x, 0, moveInput.y + edgeScrollInput.y);
        inputVector.Normalize();

        float zoomMultiplier = MoveSpeedZoomCurve.Evaluate(ZoomLevel);
        
        Vector3 targetVelocity = inputVector * MoveSpeed * zoomMultiplier;

        if (inputVector.sqrMagnitude > 0.01f)
        {
            Velocity = Vector3.MoveTowards(Velocity, targetVelocity, Acceleration * deltaTime);
        }
        else
        {
            Velocity = Vector3.MoveTowards(Velocity, Vector3.zero, Deceleration * deltaTime);
        }

        Vector3 motion = Velocity * deltaTime;

        CameraTarget.position += forward * motion.z + right * motion.x;
        
        ClampTargetToBounds();
    }

    void UpdateOrbit(float deltaTime)
    {
        Vector2 orbitInput = lookInput * (rightClickInput ? 1f : 0f);

        orbitInput *= OrbitSensitivity;

        InputAxis horizontalAxis = OrbitalFollow.HorizontalAxis;
        InputAxis verticalAxis = OrbitalFollow.VerticalAxis;

        horizontalAxis.Value = Mathf.Lerp(horizontalAxis.Value, horizontalAxis.Value + orbitInput.x, OrbitSoothing * deltaTime);
        verticalAxis.Value = Mathf.Lerp(verticalAxis.Value, verticalAxis.Value - orbitInput.y, OrbitSoothing * deltaTime);

        verticalAxis.Value = Mathf.Clamp(verticalAxis.Value, verticalAxis.Range.x, verticalAxis.Range.y);

        OrbitalFollow.HorizontalAxis = horizontalAxis;
        OrbitalFollow.VerticalAxis = verticalAxis;
    }

    void UpdateZoom(float deltaTime)
    {
        InputAxis axis = OrbitalFollow.RadialAxis;

        float targetZoomSpeed = 0f;

        if (Mathf.Abs(scrollInput.y) >= 0.01f)
        {
            targetZoomSpeed = ZoomSpeed * scrollInput.y;
        }

        CurrentZoomSpeed = Mathf.Lerp(CurrentZoomSpeed, targetZoomSpeed, ZoomSmoothing * deltaTime);

        axis.Value -= CurrentZoomSpeed;
        axis.Value = Mathf.Clamp(axis.Value, axis.Range.x, axis.Range.y);

        OrbitalFollow.RadialAxis = axis;
    }

    void UpdateEdgeScrolling()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        edgeScrollInput = Vector2.zero;

        if (mousePosition.x <= EdgeScrollingMargin)
        {
            edgeScrollInput.x = -1f;
        }
        else if(mousePosition.x >= Screen.width - EdgeScrollingMargin)
        {
            edgeScrollInput.x = 1f;
        }
        
        if (mousePosition.y <= EdgeScrollingMargin)
        {
            edgeScrollInput.y = -1f;
        }
        else if(mousePosition.y >= Screen.height - EdgeScrollingMargin)
        {
            edgeScrollInput.y = 1f;
        }
    }
    
    private void ClampTargetToBounds()
    {
        if (mapBounds == null || CameraTarget == null) return;

        // bounds в мировых координатах
        Bounds b = mapBounds.bounds;

        Vector3 p = CameraTarget.position;

        p.x = Mathf.Clamp(p.x, b.min.x + targetPadding, b.max.x - targetPadding);
        p.z = Mathf.Clamp(p.z, b.min.z + targetPadding, b.max.z - targetPadding);

        // Y можно оставить как есть (обычно таргет стоит на земле)
        CameraTarget.position = p;
    }


    #endregion
}
