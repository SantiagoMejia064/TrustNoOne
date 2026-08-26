using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class RadioFrequencyDial : MonoBehaviour
{
    private enum DialMovementMode
    {
        Rotate,
        Translate
    }

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private TaskManager taskManager;

    [Header("Task")]
    [SerializeField] private string radioTaskId = "cambiar_frecuencia_radio";
    [SerializeField] private bool resolveTaskWhenChannelBReached = true;

    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private float dragSensitivity = 0.04f;
    [SerializeField] private bool invertDrag;

    [Header("Frequency")]
    [SerializeField] private float minFrequency = 88f;
    [SerializeField] private float maxFrequency = 108f;
    [SerializeField] private float currentFrequency = 95f;
    [SerializeField] private float channelBFrequency = 101.5f;
    [SerializeField] private float channelBTolerance = 0.2f;

    [Header("Dial Movement")]
    [SerializeField] private DialMovementMode movementMode = DialMovementMode.Rotate;

    [Header("Dial Rotation")]
    [SerializeField] private Vector3 rotationAxis = Vector3.forward;
    [SerializeField] private float minAngle = -120f;
    [SerializeField] private float maxAngle = 120f;

    [Header("Dial Translation")]
    [SerializeField] private Transform minPositionPoint;
    [SerializeField] private Transform maxPositionPoint;
    [SerializeField] private Vector3 fallbackMoveAxis = Vector3.right;
    [SerializeField] private float fallbackMinOffset = -0.1f;
    [SerializeField] private float fallbackMaxOffset = 0.1f;

    [Header("Visual Feedback")]
    [SerializeField] private TMP_Text frequencyText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private string frequencySuffix = " MHz";
    [SerializeField] private string idleStatusText = "FRECUENCIA";
    [SerializeField] private string draggingStatusText = "SINTONIZANDO";
    [SerializeField] private string channelBStatusText = "CANAL B";
    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color channelBTextColor = Color.green;

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private bool isDragging;
    private bool taskResolvedByDial;

    // Guarda el estado inicial para aplicar rotacion o desplazamiento desde una base estable.
    private void Awake()
    {
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
    }

    // Ajusta la frecuencia inicial y actualiza el dial al arrancar la escena.
    private void Start()
    {
        currentFrequency = Mathf.Clamp(currentFrequency, minFrequency, maxFrequency);
        ApplyDialMovement();
        UpdateVisualFeedback();
    }

    // Corta el arrastre si el objeto se desactiva para no dejar la camara bloqueada.
    private void OnDisable()
    {
        EndDragging();
    }

    // Lee el mouse y cambia la frecuencia mientras el jugador mantiene click sobre el dial.
    private void Update()
    {
        if (Mouse.current == null)
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && IsPointingAtDial())
        {
            BeginDragging();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            EndDragging();
        }

        if (!isDragging)
        {
            return;
        }

        float mouseDeltaX = Mouse.current.delta.ReadValue().x;
        float dragDirection = invertDrag ? -1f : 1f;
        SetFrequency(currentFrequency + mouseDeltaX * dragSensitivity * dragDirection);
    }

    // Inicia el arrastre y bloquea la mirada para que mover el dial no mueva la camara.
    private void BeginDragging()
    {
        isDragging = true;
        Movimiento.SetLookBlocked(true);
        UpdateVisualFeedback();
    }

    // Termina el arrastre y devuelve el control normal de camara al jugador.
    private void EndDragging()
    {
        if (!isDragging)
        {
            return;
        }

        isDragging = false;
        Movimiento.SetLookBlocked(false);
        UpdateVisualFeedback();
    }

    // Comprueba si el centro de la camara esta apuntando al dial o a un hijo suyo.
    private bool IsPointingAtDial()
    {
        if (playerCamera == null)
        {
            return false;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        return Physics.Raycast(ray, out RaycastHit hit, interactionDistance)
            && hit.collider.GetComponentInParent<RadioFrequencyDial>() == this;
    }

    // Cambia la frecuencia dentro del rango permitido y actualiza la tarea si hace falta.
    private void SetFrequency(float newFrequency)
    {
        float previousFrequency = currentFrequency;
        currentFrequency = Mathf.Clamp(newFrequency, minFrequency, maxFrequency);

        if (Mathf.Approximately(previousFrequency, currentFrequency))
        {
            return;
        }

        ApplyDialMovement();
        UpdateVisualFeedback();
        TryResolveRadioTask();
    }

    // Convierte la frecuencia actual en movimiento fisico del dial.
    private void ApplyDialMovement()
    {
        float t = Mathf.InverseLerp(minFrequency, maxFrequency, currentFrequency);

        if (movementMode == DialMovementMode.Rotate)
        {
            ApplyDialRotation(t);
            return;
        }

        ApplyDialTranslation(t);
    }

    // Rota el dial usando el rango de angulos configurado en el inspector.
    private void ApplyDialRotation(float t)
    {
        Vector3 normalizedAxis = rotationAxis == Vector3.zero ? Vector3.forward : rotationAxis.normalized;
        float angle = Mathf.Lerp(minAngle, maxAngle, t);
        transform.localRotation = initialLocalRotation * Quaternion.AngleAxis(angle, normalizedAxis);
    }

    // Desplaza el dial entre dos puntos o usa un eje de respaldo si no hay puntos.
    private void ApplyDialTranslation(float t)
    {
        if (minPositionPoint != null && maxPositionPoint != null)
        {
            transform.position = Vector3.Lerp(minPositionPoint.position, maxPositionPoint.position, t);
            return;
        }

        Vector3 normalizedAxis = fallbackMoveAxis == Vector3.zero ? Vector3.right : fallbackMoveAxis.normalized;
        float offset = Mathf.Lerp(fallbackMinOffset, fallbackMaxOffset, t);
        transform.localPosition = initialLocalPosition + normalizedAxis * offset;
    }

    // Actualiza textos y colores para mostrar la frecuencia actual al jugador.
    private void UpdateVisualFeedback()
    {
        bool reachedChannelB = IsOnChannelB();
        Color textColor = reachedChannelB ? channelBTextColor : normalTextColor;

        if (frequencyText != null)
        {
            frequencyText.text = currentFrequency.ToString("0.0") + frequencySuffix;
            frequencyText.color = textColor;
        }

        if (statusText != null)
        {
            statusText.text = reachedChannelB
                ? channelBStatusText
                : isDragging ? draggingStatusText : idleStatusText;
            statusText.color = textColor;
        }
    }

    // Indica si la frecuencia esta suficientemente cerca del canal objetivo.
    private bool IsOnChannelB()
    {
        return Mathf.Abs(currentFrequency - channelBFrequency) <= channelBTolerance;
    }

    // Si la tarea activa es la radio y llega al canal B, la resuelve como autorizar.
    private void TryResolveRadioTask()
    {
        if (!resolveTaskWhenChannelBReached || taskManager == null)
        {
            return;
        }

        TaskData currentTask = taskManager.GetCurrentTask();

        if (currentTask == null || currentTask.taskId != radioTaskId)
        {
            return;
        }

        if (taskResolvedByDial && !taskManager.IsCurrentTaskResolved())
        {
            taskResolvedByDial = false;
        }

        if (taskResolvedByDial || !IsOnChannelB())
        {
            return;
        }

        taskResolvedByDial = true;
        taskManager.ResolveCurrentTask(DecisionType.Authorize);
    }
}
