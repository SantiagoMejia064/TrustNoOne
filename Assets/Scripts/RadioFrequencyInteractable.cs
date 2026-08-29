using UnityEngine;
using UnityEngine.InputSystem;

public enum RadioFrequencyMovementMode
{
    Rotate,
    Translate
}

public class RadioFrequencyInteractable : MonoBehaviour, IInteractable
{
    [Header("Task")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private string taskId = "Tarea7";
    [SerializeField] private BotonAutorizar botonAutorizar;
    [SerializeField] private BotonRechazar botonRechazar;

    [Header("Interaction")]
    [SerializeField] private string interactionText = "M1 - Change frequency";
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private float dragSensitivity = 0.04f;
    [SerializeField] private Camera playerCamera;

    [Header("Frequency")]
    [SerializeField] private float minFrequency = 88f;
    [SerializeField] private float maxFrequency = 108f;
    [SerializeField] private float currentFrequency = 95f;
    [SerializeField] private float channelBFrequency = 101.5f;
    [SerializeField] private float channelBTolerance = 0.2f;

    [Header("Dial Movement")]
    [SerializeField] private Transform dialVisual;
    [SerializeField] private RadioFrequencyMovementMode movementMode = RadioFrequencyMovementMode.Rotate;
    [SerializeField] private Vector3 rotationAxis = Vector3.forward;
    [SerializeField] private float minAngle = -120f;
    [SerializeField] private float maxAngle = 120f;
    [SerializeField] private Transform minPositionPoint;
    [SerializeField] private Transform maxPositionPoint;

    [Header("Sabotage Feedback")]
    [SerializeField] private GameObject[] interferenceVisuals;
    [SerializeField] private AudioSource interferenceAudio;

    private bool isDragging;
    private bool taskDone;

    // Prepara referencias y coloca el dial en su frecuencia inicial.
    private void Awake()
    {
        if (dialVisual == null)
        {
            dialVisual = transform;
        }

        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main;
        }

        ApplyDialMovement();
        SetInterferenceActive(false);
    }

    // Escucha el mouse para arrastrar el dial solo cuando esta tarea esta activa.
    private void Update()
    {
        if (!CanInteract() || Mouse.current == null)
        {
            StopDragging();
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && IsPointingAtDial())
        {
            StartDragging();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            StopDragging();
        }

        if (isDragging)
        {
            float mouseDeltaX = Mouse.current.delta.ReadValue().x;
            SetFrequency(currentFrequency + mouseDeltaX * dragSensitivity);
        }
    }

    // Libera el bloqueo de camara si el objeto se desactiva mientras se arrastra.
    private void OnDisable()
    {
        StopDragging();
    }

    // Muestra el texto de interaccion cuando el jugador mira el dial.
    public string GetInteractionText()
    {
        return interactionText;
    }

    // Permite interactuar solo durante la Tarea7 y antes de haberla completado.
    public bool CanInteract()
    {
        return !taskDone
            && taskManager != null
            && taskManager.CurrentTask != null
            && taskManager.CurrentTask.Id == taskId;
    }

    // La radio se maneja con click sostenido, asi que la tecla E no hace nada.
    public void Interact()
    {
    }

    // Inicia el arrastre del dial y bloquea el movimiento de camara.
    private void StartDragging()
    {
        isDragging = true;
        Movimiento.SetLookBlocked(true);
    }

    // Termina el arrastre del dial y devuelve el control de camara.
    private void StopDragging()
    {
        if (!isDragging)
        {
            return;
        }

        isDragging = false;
        Movimiento.SetLookBlocked(false);
    }

    // Revisa si el centro de la camara esta apuntando a este dial.
    private bool IsPointingAtDial()
    {
        if (playerCamera == null)
        {
            return false;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        return Physics.Raycast(ray, out RaycastHit hit, interactionDistance)
            && hit.collider.GetComponentInParent<RadioFrequencyInteractable>() == this;
    }

    // Cambia la frecuencia dentro del rango permitido.
    private void SetFrequency(float newFrequency)
    {
        currentFrequency = Mathf.Clamp(newFrequency, minFrequency, maxFrequency);
        ApplyDialMovement();
        TryFinishTask();
    }

    // Mueve visualmente el dial por rotacion o por desplazamiento.
    private void ApplyDialMovement()
    {
        float t = Mathf.InverseLerp(minFrequency, maxFrequency, currentFrequency);

        if (movementMode == RadioFrequencyMovementMode.Rotate)
        {
            float angle = Mathf.Lerp(minAngle, maxAngle, t);
            dialVisual.localRotation = Quaternion.AngleAxis(angle, rotationAxis.normalized);
            return;
        }

        if (minPositionPoint != null && maxPositionPoint != null)
        {
            dialVisual.position = Vector3.Lerp(minPositionPoint.position, maxPositionPoint.position, t);
        }
    }

    // Completa la accion fisica cuando la frecuencia llega al canal B.
    private void TryFinishTask()
    {
        if (Mathf.Abs(currentFrequency - channelBFrequency) > channelBTolerance)
        {
            return;
        }

        taskDone = true;
        StopDragging();
        SetInterferenceActive(true);

        botonAutorizar?.Activar();
        botonRechazar?.Desactivar();
    }

    // Activa o desactiva el feedback de radio saboteada.
    private void SetInterferenceActive(bool value)
    {
        for (int i = 0; i < interferenceVisuals.Length; i++)
        {
            if (interferenceVisuals[i] != null)
            {
                interferenceVisuals[i].SetActive(value);
            }
        }

        if (interferenceAudio == null)
        {
            return;
        }

        if (value)
        {
            interferenceAudio.Play();
        }
        else
        {
            interferenceAudio.Stop();
        }
    }
}