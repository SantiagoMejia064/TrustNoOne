
using UnityEngine;
using UnityEngine.InputSystem;

public class Movimiento : MonoBehaviour
{
    private NIS inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;
    public float speed = 5f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float lookSensitivity = 0.1f;
    [SerializeField] private float minLookAngle = -80f;
    [SerializeField] private float maxLookAngle = 80f;

    // private float fuerzaSalto = 5f;
    private float yaw;
    private float cameraPitch;
    // private Rigidbody rb;

    private void Awake()
    {
        inputActions = new NIS();
        // rb = GetComponent<Rigidbody>();
        yaw = transform.eulerAngles.y;
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
    }

    private void LateUpdate()
    {
        if (cameraTransform == null)
        {
            if (Camera.main == null)
            {
                return;
            }

            cameraTransform = Camera.main.transform;
        }

        if (Cursor.lockState != CursorLockMode.Locked)
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
}
