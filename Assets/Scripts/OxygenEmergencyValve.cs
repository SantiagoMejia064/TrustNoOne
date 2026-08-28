using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class OxygenEmergencyValve : MonoBehaviour, IInteractable
{
    private enum CircularPivotMode
    {
        ScreenCenter,
        ValveCenter
    }

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private BunkerStateManager bunkerStateManager;

    [Header("Task")]
    [SerializeField] private string oxygenValveTaskId = "abrir_valvula_oxigeno";
    [SerializeField] private bool requireTaskActiveToInteract = true;
    [SerializeField] private bool resolveTaskWhenOpened = true;

    [Header("Reserve")]
    [SerializeField] private int maxReserveUses = 3;
    [SerializeField] private int currentReserveUses = 3;
    [SerializeField] private bool consumeReserveWhenOpened = true;

    [Header("Valve")]
    [SerializeField] private bool isOpen;
    [SerializeField, Range(0f, 1f)] private float currentOpenAmount;
    [SerializeField, Range(0.1f, 1f)] private float openThreshold = 0.95f;
    [SerializeField] private bool autoCloseValve = true;
    [SerializeField] private float autoCloseDelay = 1.5f;
    [SerializeField] private bool returnToClosedWhenReleasedEarly = true;

    [Header("Circular Drag")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private CircularPivotMode circularPivotMode = CircularPivotMode.ScreenCenter;
    [SerializeField] private float requiredTurnDegrees = 360f;
    [SerializeField] private float circularSensitivity = 1f;
    [SerializeField] private bool invertDrag;
    [SerializeField] private bool allowReverseTurn = true;
    [SerializeField] private float minimumMouseDistanceFromPivot = 25f;
    [SerializeField] private float virtualCursorStartRadius = 80f;
    [SerializeField] private float virtualCursorSpeed = 1f;

    [Header("Interaction Text")]
    [SerializeField] private string rotateText = "M1 - GIRAR VALVULA";
    [SerializeField] private string rotatingText = "GIRANDO VALVULA";

    [Header("Visuals")]
    [SerializeField] private Transform valveHandle;
    [SerializeField] private Vector3 baseLocalEulerAngles;
    [SerializeField] private Vector3 turnAxis = Vector3.forward;
    [SerializeField] private float closedTurnDegrees;
    [SerializeField] private float openTurnDegrees = -360f;
    [SerializeField] private ReserveLightIndicator[] reserveAvailableIndicators;
    [SerializeField] private ReserveLightIndicator[] reserveEmptyIndicators;

    [Header("Panel Feedback")]
    [SerializeField] private TMP_Text oxygenStateText;
    [SerializeField] private TMP_Text reserveText;
    [SerializeField] private Transform pressureNeedle;
    [SerializeField] private float stableNeedleAngle = -40f;
    [SerializeField] private float lowNeedleAngle = 0f;
    [SerializeField] private float criticalNeedleAngle = 40f;
    [SerializeField] private float collapseNeedleAngle = 70f;

    private bool isDragging;
    private bool taskResolvedByValve;
    private Coroutine autoCloseRoutine;
    private Vector2 circularPivotScreenPosition;
    private Vector2 virtualCircularMousePosition;
    private float previousCircularMouseAngle;
    private float accumulatedTurnDegrees;
    private bool hasPreviousCircularMouseAngle;

    // Escucha cambios de tarea y oxigeno para mantener el panel actualizado.
    private void OnEnable()
    {
        if (taskManager != null)
        {
            taskManager.TaskShown += HandleTaskShown;
        }

        if (bunkerStateManager != null)
        {
            bunkerStateManager.onOxygenChanged.AddListener(UpdateVisualFeedback);
        }
    }

    // Cancela suscripciones, corutinas y arrastre al desactivar el objeto.
    private void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.TaskShown -= HandleTaskShown;
        }

        if (bunkerStateManager != null)
        {
            bunkerStateManager.onOxygenChanged.RemoveListener(UpdateVisualFeedback);
        }

        StopAutoCloseRoutine();
        EndDragging(false);
    }

    // Ajusta la reserva inicial y aplica el estado visual de la valvula.
    private void Start()
    {
        currentReserveUses = Mathf.Clamp(currentReserveUses, 0, maxReserveUses);
        currentOpenAmount = isOpen ? 1f : Mathf.Clamp01(currentOpenAmount);
        accumulatedTurnDegrees = currentOpenAmount * GetSafeRequiredTurnDegrees();
        UpdateVisualFeedback();
    }

    // Lee el mouse para agarrar la valvula y girarla con click sostenido.
    private void Update()
    {
        if (Mouse.current == null)
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && CanInteract() && IsPointingAtValve())
        {
            BeginDragging();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            EndDragging(true);
        }

        if (!isDragging)
        {
            return;
        }

        ApplyCircularMouseRotation();
        UpdateVisualFeedback();

        if (currentOpenAmount >= openThreshold)
        {
            EndDragging(true);
        }
    }

    // Devuelve el texto que se muestra al mirar o girar la valvula.
    public string GetInteractionText()
    {
        return isDragging ? rotatingText : rotateText;
    }
    
    // Permite interactuar solo si hay reserva, la valvula esta cerrada y la tarea lo permite.
    public bool CanInteract()
    {
        return currentReserveUses > 0 && !isOpen && CanUseValveNow();
    }

    // La tecla E no abre la valvula; la accion real es agarrarla con click y girarla.
    public void Interact()
    {
        Debug.Log("Para abrir la valvula, mantenga click izquierdo y gire con el mouse.");
    }

    // Cierra la valvula sin recuperar la reserva gastada.
    public void CloseValve()
    {
        isOpen = false;
        currentOpenAmount = 0f;
        accumulatedTurnDegrees = 0f;
        StopAutoCloseRoutine();
        UpdateVisualFeedback();
    }

    // Rellena la reserva desde un evento externo o para pruebas.
    public void RefillReserve()
    {
        currentReserveUses = maxReserveUses;
        UpdateVisualFeedback();
    }

    // Inicia el giro manual y bloquea la camara mientras se manipula la valvula.
    private void BeginDragging()
    {
        isDragging = true;
        StopAutoCloseRoutine();
        PrepareCircularDrag();
        Movimiento.SetLookBlocked(true);
        UpdateVisualFeedback();
    }

    // Termina el giro manual y decide si vuelve a cerrarse o completa la apertura.
    private void EndDragging(bool shouldEvaluate)
    {
        if (!isDragging)
        {
            return;
        }

        isDragging = false;
        hasPreviousCircularMouseAngle = false;
        Movimiento.SetLookBlocked(false);

        if (shouldEvaluate && currentOpenAmount >= openThreshold)
        {
            CompleteValveOpening();
            return;
        }

        if (returnToClosedWhenReleasedEarly && !isOpen)
        {
            currentOpenAmount = 0f;
            accumulatedTurnDegrees = 0f;
        }

        UpdateVisualFeedback();
    }

    // Mide el angulo del mouse virtual alrededor del pivote para exigir un giro circular real.
    private void ApplyCircularMouseRotation()
    {
        Vector2 mousePosition = GetVirtualCircularMousePosition();
        Vector2 directionFromPivot = mousePosition - circularPivotScreenPosition;

        if (directionFromPivot.magnitude < minimumMouseDistanceFromPivot)
        {
            return;
        }

        float currentMouseAngle = Mathf.Atan2(directionFromPivot.y, directionFromPivot.x) * Mathf.Rad2Deg;

        if (!hasPreviousCircularMouseAngle)
        {
            previousCircularMouseAngle = currentMouseAngle;
            hasPreviousCircularMouseAngle = true;
            return;
        }

        float angleDelta = Mathf.DeltaAngle(previousCircularMouseAngle, currentMouseAngle);
        previousCircularMouseAngle = currentMouseAngle;

        if (invertDrag)
        {
            angleDelta *= -1f;
        }

        angleDelta *= circularSensitivity;

        if (!allowReverseTurn && angleDelta < 0f)
        {
            angleDelta = 0f;
        }

        accumulatedTurnDegrees = Mathf.Clamp(accumulatedTurnDegrees + angleDelta, 0f, GetSafeRequiredTurnDegrees());
        currentOpenAmount = accumulatedTurnDegrees / GetSafeRequiredTurnDegrees();
    }

    // Define el centro alrededor del cual el mouse virtual debe hacer el movimiento circular.
    private void PrepareCircularDrag()
    {
        accumulatedTurnDegrees = currentOpenAmount * GetSafeRequiredTurnDegrees();
        hasPreviousCircularMouseAngle = false;

        if (circularPivotMode == CircularPivotMode.ValveCenter && playerCamera != null)
        {
            Transform pivotTransform = valveHandle != null ? valveHandle : transform;
            Vector3 screenPosition = playerCamera.WorldToScreenPoint(pivotTransform.position);

            if (screenPosition.z > 0f)
            {
                circularPivotScreenPosition = screenPosition;
                ResetVirtualCircularMousePosition();
                return;
            }
        }

        circularPivotScreenPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        ResetVirtualCircularMousePosition();
    }

    // Mueve un cursor virtual usando el delta del mouse, aunque el cursor real siga bloqueado.
    private Vector2 GetVirtualCircularMousePosition()
    {
        virtualCircularMousePosition += Mouse.current.delta.ReadValue() * virtualCursorSpeed;
        virtualCircularMousePosition.x = Mathf.Clamp(virtualCircularMousePosition.x, 0f, Screen.width);
        virtualCircularMousePosition.y = Mathf.Clamp(virtualCircularMousePosition.y, 0f, Screen.height);
        return virtualCircularMousePosition;
    }

    // Coloca el cursor virtual a un lado del pivote para empezar el calculo circular.
    private void ResetVirtualCircularMousePosition()
    {
        virtualCircularMousePosition = circularPivotScreenPosition
            + Vector2.right * Mathf.Max(minimumMouseDistanceFromPivot + 5f, virtualCursorStartRadius);
    }

    // Consume reserva, deja la valvula abierta y resuelve la tarea si esta activa.
    private void CompleteValveOpening()
    {
        if (isOpen)
        {
            return;
        }

        isOpen = true;
        currentOpenAmount = 1f;
        accumulatedTurnDegrees = GetSafeRequiredTurnDegrees();

        if (consumeReserveWhenOpened)
        {
            currentReserveUses = Mathf.Max(0, currentReserveUses - 1);
        }

        UpdateVisualFeedback();
        TryResolveOxygenValveTask();

        if (autoCloseValve)
        {
            StopAutoCloseRoutine();
            autoCloseRoutine = StartCoroutine(CloseValveAfterDelay());
        }
    }

    // Reinicia la proteccion para que la tarea pueda resolverse si aparece de nuevo.
    private void HandleTaskShown(TaskData task)
    {
        if (task != null && task.taskId == oxygenValveTaskId)
        {
            taskResolvedByValve = false;
        }

        UpdateVisualFeedback();
    }

    // Espera unos segundos y vuelve a cerrar la valvula.
    private IEnumerator CloseValveAfterDelay()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, autoCloseDelay));
        autoCloseRoutine = null;
        isOpen = false;
        currentOpenAmount = 0f;
        accumulatedTurnDegrees = 0f;
        UpdateVisualFeedback();
    }

    // Detiene el autocierre si ya no se necesita.
    private void StopAutoCloseRoutine()
    {
        if (autoCloseRoutine == null)
        {
            return;
        }

        StopCoroutine(autoCloseRoutine);
        autoCloseRoutine = null;
    }

    // Verifica si el centro de la camara apunta a esta valvula.
    private bool IsPointingAtValve()
    {
        if (playerCamera == null)
        {
            return false;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        return Physics.Raycast(ray, out RaycastHit hit, interactionDistance)
            && hit.collider.GetComponentInParent<OxygenEmergencyValve>() == this;
    }

    // Verifica si la valvula puede usarse con la tarea actual.
    private bool CanUseValveNow()
    {
        if (!requireTaskActiveToInteract)
        {
            return true;
        }

        TaskData currentTask = taskManager != null ? taskManager.GetCurrentTask() : null;
        return currentTask != null && currentTask.taskId == oxygenValveTaskId && !taskManager.IsCurrentTaskResolved();
    }

    // Si la tarea activa es esta valvula, abrirla cuenta como autorizar.
    private void TryResolveOxygenValveTask()
    {
        if (!resolveTaskWhenOpened || taskResolvedByValve || taskManager == null)
        {
            return;
        }

        TaskData currentTask = taskManager.GetCurrentTask();

        if (currentTask == null || currentTask.taskId != oxygenValveTaskId)
        {
            return;
        }

        taskResolvedByValve = true;
        taskManager.ResolveCurrentTask(DecisionType.Authorize);
    }

    // Aplica la rotacion de la manija y los visuales de reserva.
    private void ApplyValveVisuals()
    {
        if (valveHandle != null)
        {
            Vector3 normalizedAxis = turnAxis == Vector3.zero ? Vector3.forward : turnAxis.normalized;
            float turnDegrees = Mathf.Lerp(closedTurnDegrees, openTurnDegrees, currentOpenAmount);
            valveHandle.localRotation = Quaternion.Euler(baseLocalEulerAngles) * Quaternion.AngleAxis(turnDegrees, normalizedAxis);
        }

        bool hasReserve = currentReserveUses > 0;
        SetReserveIndicatorState(reserveAvailableIndicators, hasReserve);
        SetReserveIndicatorState(reserveEmptyIndicators, !hasReserve);
    }

    // Enciende o apaga una lista de bombillos de reserva.
    private void SetReserveIndicatorState(ReserveLightIndicator[] indicators, bool isLit)
    {
        if (indicators == null)
        {
            return;
        }

        foreach (ReserveLightIndicator indicator in indicators)
        {
            if (indicator == null)
            {
                continue;
            }

            indicator.SetLit(isLit);
        }
    }

    // Refresca textos del panel y la aguja del medidor de oxigeno.
    private void UpdateVisualFeedback()
    {
        BunkerSystemState oxygenState = GetCurrentOxygenState();

        if (oxygenStateText != null)
        {
            oxygenStateText.text = "OXIGENO: " + oxygenState.ToString().ToUpperInvariant();
        }

        if (reserveText != null)
        {
            reserveText.text = "RESERVA: " + currentReserveUses + "/" + maxReserveUses;
        }

        if (pressureNeedle != null)
        {
            pressureNeedle.localRotation = Quaternion.Euler(0f, 0f, GetNeedleAngle(oxygenState));
        }

        ApplyValveVisuals();
    }

    // Lee el estado actual de oxigeno del bunker.
    private BunkerSystemState GetCurrentOxygenState()
    {
        if (bunkerStateManager == null)
        {
            return BunkerSystemState.Stable;
        }

        return bunkerStateManager.GetSystemState(BunkerSystemType.Oxygen);
    }

    // Evita divisiones por cero si el valor se configura mal en el Inspector.
    private float GetSafeRequiredTurnDegrees()
    {
        return Mathf.Max(0.01f, requiredTurnDegrees);
    }

    // Convierte el estado de oxigeno en angulo para una aguja visual.
    private float GetNeedleAngle(BunkerSystemState oxygenState)
    {
        switch (oxygenState)
        {
            case BunkerSystemState.Low:
                return lowNeedleAngle;
            case BunkerSystemState.Critical:
                return criticalNeedleAngle;
            case BunkerSystemState.Collapse:
                return collapseNeedleAngle;
            default:
                return stableNeedleAngle;
        }
    }
}