using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movimiento : MonoBehaviour
{
    public static bool IsLookBlocked { get; private set; }

    private NIS inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;
    public float speed = 5f;

    [Header("Look")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float lookSensitivity = 0.1f;
    [SerializeField] private float minLookAngle = -80f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("Camera Zones")]
    [SerializeField] private bool enableCameraZones = true;
    [SerializeField] private float zoneInteractionDistance = 10f;
    [SerializeField] private LayerMask movementZoneLayer = ~0;
    [SerializeField] private float defaultZoneMoveDuration = 0.55f;
    [SerializeField] private bool rotateTowardZone = true;

    // private float fuerzaSalto = 5f;
    private float yaw;
    private float cameraPitch;
    private Coroutine zoneMoveRoutine;
    // private Rigidbody rb;

    public static void SetLookBlocked(bool isBlocked)
    {
        IsLookBlocked = isBlocked;
    }

    private void Awake()
    {
        inputActions = new NIS();
        // rb = GetComponent<Rigidbody>();
        SyncLookWithCurrentTransforms();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        // Movimiento WASD desactivado temporalmente.
        // inputActions.Player.Move.performed += OnMove;
        // inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.canceled += OnLook;

        // Salto desactivado temporalmente.
        // inputActions.Player.Jump.performed += OnJump;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        // Movimiento WASD desactivado temporalmente.
        // inputActions.Player.Move.performed -= OnMove;
        // inputActions.Player.Move.canceled -= OnMove;
        inputActions.Player.Look.performed -= OnLook;
        inputActions.Player.Look.canceled -= OnLook;

        // Salto desactivado temporalmente.
        // inputActions.Player.Jump.performed -= OnJump;

        inputActions.Player.Disable();

        if (zoneMoveRoutine != null)
        {
            StopCoroutine(zoneMoveRoutine);
            zoneMoveRoutine = null;
        }

        SetLookBlocked(false);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Movimiento WASD desactivado temporalmente.
    // private void OnMove(InputAction.CallbackContext context)
    // {
    //     moveInput = context.ReadValue<Vector2>();
    // }

    private void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    // Salto desactivado temporalmente.
    // private void OnJump(InputAction.CallbackContext context)
    // {
    //     Debug.Log("Saltar");
    //     rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
    // }

    private void Update()
    {
        // Movimiento WASD desactivado temporalmente.
        // Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);
        // transform.Translate(movement * speed * Time.deltaTime);

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        HandleCameraZoneClick();
    }

    private void LateUpdate()
    {
        if (!TryResolveCameraTransform())
        {
            return;
        }

        if (Cursor.lockState != CursorLockMode.Locked || IsLookBlocked)
        {
            return;
        }

        yaw += lookInput.x * lookSensitivity;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        cameraPitch = Mathf.Clamp(
            cameraPitch - lookInput.y * lookSensitivity,
            minLookAngle,
            maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void HandleCameraZoneClick()
    {
        if (!enableCameraZones || zoneMoveRoutine != null || IsLookBlocked)
        {
            return;
        }

        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        CameraMoveZone zone = GetCameraMoveZoneInView();

        if (zone == null || !zone.CanUse || zone.DestinationPoint == null)
        {
            return;
        }

        zoneMoveRoutine = StartCoroutine(MoveToCameraZone(zone));
    }

    private CameraMoveZone GetCameraMoveZoneInView()
    {
        if (!TryResolveCameraTransform())
        {
            return null;
        }

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, zoneInteractionDistance, movementZoneLayer, QueryTriggerInteraction.Collide))
        {
            return null;
        }

        return hit.collider.GetComponentInParent<CameraMoveZone>();
    }

    private IEnumerator MoveToCameraZone(CameraMoveZone zone)
    {
        SetLookBlocked(true);

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = zone.DestinationPoint.position;
        float startYaw = yaw;
        float startPitch = cameraPitch;
        float targetYaw = GetZoneTargetYaw(zone, targetPosition, startYaw);
        float targetPitch = GetZoneTargetPitch(zone, targetPosition, targetYaw, startPitch);
        float duration = zone.HasCustomMoveDuration ? zone.CustomMoveDuration : defaultZoneMoveDuration;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            if (rotateTowardZone)
            {
                SetLookAngles(Mathf.LerpAngle(startYaw, targetYaw, t), Mathf.Lerp(startPitch, targetPitch, t));
            }

            yield return null;
        }

        transform.position = targetPosition;

        if (rotateTowardZone)
        {
            SetLookAngles(targetYaw, targetPitch);
        }

        SetLookBlocked(false);
        zoneMoveRoutine = null;
    }

    private float GetZoneTargetYaw(CameraMoveZone zone, Vector3 targetPosition, float fallbackYaw)
    {
        if (zone.LookAtTarget != null)
        {
            Vector3 direction = zone.LookAtTarget.position - GetCameraPositionAtRootPosition(targetPosition);
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.0001f)
            {
                return Quaternion.LookRotation(direction).eulerAngles.y;
            }
        }

        if (zone.UseDestinationYaw)
        {
            return zone.DestinationPoint.eulerAngles.y;
        }

        return fallbackYaw;
    }

    private float GetZoneTargetPitch(CameraMoveZone zone, Vector3 targetPosition, float targetYaw, float fallbackPitch)
    {
        if (zone.LookAtTarget != null)
        {
            Vector3 direction = zone.LookAtTarget.position - GetCameraPositionAtRootPosition(targetPosition);
            Quaternion targetBodyRotation = Quaternion.Euler(0f, targetYaw, 0f);
            Vector3 localDirection = Quaternion.Inverse(targetBodyRotation) * direction.normalized;
            float pitch = NormalizeAngle(Quaternion.LookRotation(localDirection).eulerAngles.x);
            return Mathf.Clamp(pitch, minLookAngle, maxLookAngle);
        }

        if (zone.OverrideCameraPitch)
        {
            return Mathf.Clamp(zone.TargetCameraPitch, minLookAngle, maxLookAngle);
        }

        return fallbackPitch;
    }

    private Vector3 GetCameraPositionAtRootPosition(Vector3 rootPosition)
    {
        if (!TryResolveCameraTransform())
        {
            return rootPosition;
        }

        return rootPosition + transform.rotation * cameraTransform.localPosition;
    }

    private void SetLookAngles(float newYaw, float newCameraPitch)
    {
        yaw = newYaw;
        cameraPitch = Mathf.Clamp(newCameraPitch, minLookAngle, maxLookAngle);
        lookInput = Vector2.zero;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        if (TryResolveCameraTransform())
        {
            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }
    }

    private void SyncLookWithCurrentTransforms()
    {
        yaw = transform.eulerAngles.y;

        if (TryResolveCameraTransform())
        {
            cameraPitch = Mathf.Clamp(NormalizeAngle(cameraTransform.localEulerAngles.x), minLookAngle, maxLookAngle);
        }
    }

    private bool TryResolveCameraTransform()
    {
        if (cameraTransform != null)
        {
            return true;
        }

        if (Camera.main == null)
        {
            return false;
        }

        cameraTransform = Camera.main.transform;
        return true;
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180f)
        {
            angle -= 360f;
        }

        while (angle < -180f)
        {
            angle += 360f;
        }

        return angle;
    }
}
