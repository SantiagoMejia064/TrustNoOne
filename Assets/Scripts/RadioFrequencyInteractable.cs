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

    [Header("Frequency Indicator")]
    [SerializeField] private Transform frequencyIndicator;
    [SerializeField] private Transform indicatorMinPoint;
    [SerializeField] private Transform indicatorMaxPoint;

    [Header("Sabotage Feedback")]
    [SerializeField] private GameObject[] interferenceVisuals;
    [SerializeField] private AudioSource interferenceAudio;

    [Header("Radio Audio")]
    [SerializeField] private AudioSource tuningAudio;
    [SerializeField] private AudioSource correctFrequencyAudio;
    [SerializeField] private AudioSource rejectedStaticAudio;

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

    // Se suscribe al TaskManager para saber si el jugador rechaza esta tarea.
    private void OnEnable()
    {
        if (taskManager != null)
        {
            taskManager.onTaskResolved.AddListener(OnTaskResolved);
        }
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

    // Libera eventos, sonidos y bloqueo de camara si el objeto se desactiva.
    private void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.onTaskResolved.RemoveListener(OnTaskResolved);
        }

        StopDragging();
        StopAudio(tuningAudio);
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
        PlayLoopAudio(tuningAudio);
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
        StopAudio(tuningAudio);
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

    // Mueve visualmente el dial y la aguja de frecuencia.
    private void ApplyDialMovement()
    {
        float t = Mathf.InverseLerp(minFrequency, maxFrequency, currentFrequency);
        ApplyDialVisual(t);
        ApplyFrequencyIndicator(t);
    }

    // Mueve el dial por rotacion o desplazamiento segun el modo elegido.
    private void ApplyDialVisual(float t)
    {
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

    // Desplaza la aguja entre dos puntos para mostrar la frecuencia actual.
    private void ApplyFrequencyIndicator(float t)
    {
        if (frequencyIndicator == null || indicatorMinPoint == null || indicatorMaxPoint == null)
        {
            return;
        }

        frequencyIndicator.position = Vector3.Lerp(indicatorMinPoint.position, indicatorMaxPoint.position, t);
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
        PlayOneShotAudio(correctFrequencyAudio);
        StopAudio(rejectedStaticAudio);

        botonAutorizar?.Activar();
        botonRechazar?.Desactivar();
    }

    // Reacciona cuando la tarea se resuelve desde Autorizar o Rechazar.
    private void OnTaskResolved(TaskData task, DecisionType decision)
    {
        if (task == null || task.Id != taskId)
        {
            return;
        }

        if (decision == DecisionType.Reject)
        {
            StopDragging();
            PlayLoopAudio(rejectedStaticAudio);
        }
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

    // Reproduce un sonido en loop si no estaba sonando.
    private void PlayLoopAudio(AudioSource audioSource)
    {
        if (audioSource == null || audioSource.isPlaying)
        {
            return;
        }

        audioSource.loop = true;
        audioSource.Play();
    }

    // Reproduce un sonido una sola vez desde el inicio.
    private void PlayOneShotAudio(AudioSource audioSource)
    {
        if (audioSource == null)
        {
            return;
        }

        audioSource.loop = false;
        audioSource.Play();
    }

    // Detiene un sonido si esta sonando.
    private void StopAudio(AudioSource audioSource)
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}