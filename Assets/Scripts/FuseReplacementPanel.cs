using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class FuseReplacementPanel : MonoBehaviour
{
    private enum DragMovementMode
    {
        Free,
        Guided
    }

    private enum DragInputAxis
    {
        Horizontal,
        Vertical
    }

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private TaskManager taskManager;

    [Header("Task")]
    [SerializeField] private string fuseTaskId = "sustituir_fusible_luces";
    [SerializeField] private bool requireTaskActiveToInteract = true;
    [SerializeField] private bool resolveTaskWhenReplacementInstalled = true;

    [Header("Fuse Pieces")]
    [SerializeField] private FusePiece oldFuse;
    [SerializeField] private FusePiece replacementFuse;

    [Header("Movement Points")]
    [SerializeField] private Transform oldFuseInstalledPoint;
    [SerializeField] private Transform oldFuseRemovedPoint;
    [SerializeField] private Transform replacementStartPoint;
    [SerializeField] private Transform replacementInstalledPoint;

    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private bool invertDrag;
    [SerializeField] private bool returnToStartWhenReleasedFar = true;

    [Header("Free Drag")]
    [SerializeField] private DragMovementMode dragMovementMode = DragMovementMode.Free;
    [SerializeField] private Transform dragAxesReference;
    [SerializeField] private float freeDragSensitivity = 0.004f;
    [SerializeField] private float maxFreeDragDistance = 0.75f;
    [SerializeField] private float snapDistance = 0.12f;
    [SerializeField] private bool rotateTowardTargetWhileClose = true;

    [Header("Old Fuse Click Removal")]
    [SerializeField] private bool dropOldFuseOnClick = true;
    [SerializeField] private float oldFuseOutwardSpeed = 0.45f;
    [SerializeField] private float oldFuseDownwardSpeed = 0.25f;
    [SerializeField] private float oldFuseThrowTorque = 5f;
    [SerializeField] private float oldFuseDisappearDelay = 4f;

    [Header("Guided Drag")]
    [SerializeField] private float guidedDragSensitivity = 0.004f;
    [SerializeField] private DragInputAxis guidedDragInputAxis = DragInputAxis.Vertical;
    [SerializeField, Range(0.5f, 1f)] private float guidedSnapCompletionThreshold = 0.85f;

    [Header("Visual Feedback")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private string waitingOldFuseText = "RETIRE EL FUSIBLE DANADO";
    [SerializeField] private string waitingReplacementText = "INSERTE EL FUSIBLE NUEVO";
    [SerializeField] private string completedText = "FUSIBLE SUSTITUIDO";
    [SerializeField] private string inactiveText = "PANEL B-3";
    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color completedTextColor = Color.green;

    private FusePiece draggedFuse;
    private Transform dragStartPoint;
    private Transform dragEndPoint;
    private float guidedDragProgress;
    private bool oldFuseRemoved;
    private bool replacementInstalled;
    private bool taskResolvedByPanel;

    // Escucha eventos de tareas para activar o reiniciar el panel cuando corresponda.
    private void OnEnable()
    {
        if (taskManager != null)
        {
            taskManager.TaskShown += HandleTaskShown;
            taskManager.TaskResolved += HandleTaskResolved;
        }
    }

    // Deja de escuchar eventos y suelta cualquier fusible que estuviera en arrastre.
    private void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.TaskShown -= HandleTaskShown;
            taskManager.TaskResolved -= HandleTaskResolved;
        }

        EndDragging(false);
    }

    // Coloca los fusibles en su estado inicial al arrancar.
    private void Start()
    {
        ResetPanelState();
    }

    // Lee el click para retirar el fusible viejo o arrastrar el nuevo.
    private void Update()
    {
        if (Mouse.current == null)
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryHandleMouseDown();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            EndDragging(true);
        }

        if (draggedFuse == null)
        {
            return;
        }

        if (dragMovementMode == DragMovementMode.Guided)
        {
            ApplyGuidedDrag();
        }
        else
        {
            ApplyFreeDrag();
        }
    }

    // Decide que hacer al presionar click sobre un fusible del panel.
    private void TryHandleMouseDown()
    {
        if (draggedFuse != null || !CanUsePanelNow())
        {
            return;
        }

        FusePiece fuse = GetFuseInView();

        if (fuse == null)
        {
            return;
        }

        if (fuse == oldFuse && !oldFuseRemoved)
        {
            RemoveOldFuseByClick();
            return;
        }

        if (fuse == replacementFuse && oldFuseRemoved && !replacementInstalled)
        {
            BeginDragging(fuse, replacementStartPoint, GetReplacementInstalledPoint());
        }
    }

    // Inicia el arrastre del fusible nuevo y bloquea la camara mientras se manipula.
    private void BeginDragging(FusePiece fuse, Transform startPoint, Transform endPoint)
    {
        if (fuse == null || startPoint == null || endPoint == null)
        {
            return;
        }

        draggedFuse = fuse;
        dragStartPoint = startPoint;
        dragEndPoint = endPoint;
        guidedDragProgress = 0f;
        draggedFuse.SetPhysicsEnabled(false);
        Movimiento.SetLookBlocked(true);
    }

    // Suelta el fusible nuevo y lo encaja solo si esta cerca del punto correcto.
    private void EndDragging(bool shouldSnap)
    {
        if (draggedFuse == null)
        {
            return;
        }

        FusePiece releasedFuse = draggedFuse;
        bool completedDrag = shouldSnap && IsDraggedFuseCloseEnoughToTarget();

        if (completedDrag)
        {
            SnapFuseToPoint(releasedFuse, dragEndPoint);
        }
        else if (returnToStartWhenReleasedFar)
        {
            SnapFuseToPoint(releasedFuse, dragStartPoint);
        }

        draggedFuse = null;
        dragStartPoint = null;
        dragEndPoint = null;
        guidedDragProgress = 0f;
        Movimiento.SetLookBlocked(false);

        if (completedDrag)
        {
            CompleteFuseStep(releasedFuse);
        }

        UpdateVisualFeedback();
    }

    // Retira el fusible viejo con un click y lo hace caer o desaparecer.
    private void RemoveOldFuseByClick()
    {
        if (oldFuse == null)
        {
            return;
        }

        oldFuseRemoved = true;

        if (dropOldFuseOnClick)
        {
            Vector3 dropVelocity = GetOldFuseDropVelocity();
            oldFuse.Throw(dropVelocity, oldFuseThrowTorque, oldFuseDisappearDelay);
        }
        else
        {
            oldFuse.gameObject.SetActive(false);
        }

        UpdateVisualFeedback();
    }

    // Completa la tarea cuando el fusible nuevo queda instalado.
    private void CompleteFuseStep(FusePiece fuse)
    {
        if (fuse == replacementFuse)
        {
            replacementInstalled = true;
            TryResolveFuseTask();
        }
    }

    // Reinicia el panel cuando aparece la tarea de sustituir fusible.
    private void HandleTaskShown(TaskData task)
    {
        if (task != null && task.taskId == fuseTaskId)
        {
            ResetPanelState();
            return;
        }

        UpdateVisualFeedback();
    }

    // Refresca el texto del panel cuando la tarea se resuelve.
    private void HandleTaskResolved(TaskData task, bool wasCorrect)
    {
        if (task != null && task.taskId == fuseTaskId)
        {
            UpdateVisualFeedback();
        }
    }

    // Devuelve el panel a su estado inicial para poder jugar la tarea desde cero.
    private void ResetPanelState()
    {
        EndDragging(false);
        oldFuseRemoved = false;
        replacementInstalled = false;
        taskResolvedByPanel = false;
        ResetFuseAtPoint(oldFuse, oldFuseInstalledPoint);
        ResetFuseAtPoint(replacementFuse, replacementStartPoint);
        UpdateVisualFeedback();
    }

    // Busca que fusible esta apuntando el jugador desde el centro de la camara.
    private FusePiece GetFuseInView()
    {
        if (playerCamera == null)
        {
            return null;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            return null;
        }

        return hit.collider.GetComponentInParent<FusePiece>();
    }

    // Comprueba si el panel puede usarse segun la tarea actual.
    private bool CanUsePanelNow()
    {
        if (!requireTaskActiveToInteract)
        {
            return true;
        }

        TaskData currentTask = taskManager != null ? taskManager.GetCurrentTask() : null;
        return currentTask != null && currentTask.taskId == fuseTaskId && !taskManager.IsCurrentTaskResolved();
    }

    // Mueve el fusible por un carril entre inicio y destino.
    private void ApplyGuidedDrag()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float inputDelta = guidedDragInputAxis == DragInputAxis.Horizontal ? mouseDelta.x : mouseDelta.y;
        float dragDirection = invertDrag ? -1f : 1f;
        guidedDragProgress = Mathf.Clamp01(guidedDragProgress + inputDelta * guidedDragSensitivity * dragDirection);

        draggedFuse.transform.position = Vector3.Lerp(dragStartPoint.position, dragEndPoint.position, guidedDragProgress);
        draggedFuse.transform.rotation = Quaternion.Slerp(dragStartPoint.rotation, dragEndPoint.rotation, guidedDragProgress);
    }

    // Mueve el fusible libremente usando los ejes de la camara o del panel.
    private void ApplyFreeDrag()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float dragDirection = invertDrag ? -1f : 1f;
        Vector3 horizontalAxis = GetDragHorizontalAxis();
        Vector3 verticalAxis = GetDragVerticalAxis();
        Vector3 movement = (horizontalAxis * mouseDelta.x + verticalAxis * mouseDelta.y) * freeDragSensitivity * dragDirection;
        Vector3 targetPosition = draggedFuse.transform.position + movement;

        if (maxFreeDragDistance > 0f && dragStartPoint != null)
        {
            Vector3 offsetFromStart = targetPosition - dragStartPoint.position;

            if (offsetFromStart.magnitude > maxFreeDragDistance)
            {
                targetPosition = dragStartPoint.position + offsetFromStart.normalized * maxFreeDragDistance;
            }
        }

        draggedFuse.transform.position = targetPosition;

        if (rotateTowardTargetWhileClose && dragEndPoint != null)
        {
            float distanceToTarget = Vector3.Distance(draggedFuse.transform.position, dragEndPoint.position);
            float rotationBlend = Mathf.InverseLerp(snapDistance * 3f, snapDistance, distanceToTarget);
            draggedFuse.transform.rotation = Quaternion.Slerp(draggedFuse.transform.rotation, dragEndPoint.rotation, rotationBlend);
        }
    }

    // Decide si el fusible nuevo esta bastante cerca para encajar.
    private bool IsDraggedFuseCloseEnoughToTarget()
    {
        if (dragMovementMode == DragMovementMode.Guided)
        {
            return guidedDragProgress >= guidedSnapCompletionThreshold;
        }

        if (draggedFuse == null || dragEndPoint == null)
        {
            return false;
        }

        return Vector3.Distance(draggedFuse.transform.position, dragEndPoint.position) <= snapDistance;
    }

    // Calcula la velocidad inicial con la que cae el fusible viejo.
    private Vector3 GetOldFuseDropVelocity()
    {
        Vector3 velocity = GetFallbackDropDirection() * oldFuseOutwardSpeed;
        velocity += Vector3.down * oldFuseDownwardSpeed;
        return velocity;
    }

    // Elige una direccion de salida para que el fusible viejo no caiga dentro del panel.
    private Vector3 GetFallbackDropDirection()
    {
        if (playerCamera != null)
        {
            return -playerCamera.transform.forward;
        }

        return -transform.forward;
    }

    // Devuelve el eje horizontal usado para arrastrar el fusible nuevo.
    private Vector3 GetDragHorizontalAxis()
    {
        if (dragAxesReference != null)
        {
            return dragAxesReference.right;
        }

        if (playerCamera != null)
        {
            return playerCamera.transform.right;
        }

        return transform.right;
    }

    // Devuelve el eje vertical usado para arrastrar el fusible nuevo.
    private Vector3 GetDragVerticalAxis()
    {
        if (dragAxesReference != null)
        {
            return dragAxesReference.up;
        }

        if (playerCamera != null)
        {
            return playerCamera.transform.up;
        }

        return transform.up;
    }

    // Reactiva un fusible y lo coloca en un punto fijo del panel.
    private void ResetFuseAtPoint(FusePiece fuse, Transform point)
    {
        if (fuse == null || point == null)
        {
            return;
        }

        fuse.ResetPiece();
        SnapFuseToPoint(fuse, point);
    }

    // Encaja un fusible en un punto y apaga sus fisicas.
    private void SnapFuseToPoint(FusePiece fuse, Transform point)
    {
        if (fuse == null || point == null)
        {
            return;
        }

        fuse.SetPhysicsEnabled(false);
        fuse.transform.SetPositionAndRotation(point.position, point.rotation);
    }

    // Devuelve el punto donde debe quedar instalado el fusible nuevo.
    private Transform GetReplacementInstalledPoint()
    {
        return replacementInstalledPoint != null ? replacementInstalledPoint : oldFuseInstalledPoint;
    }

    // Resuelve la tarea como autorizar cuando el fusible nuevo fue instalado.
    private void TryResolveFuseTask()
    {
        if (!resolveTaskWhenReplacementInstalled || taskResolvedByPanel || taskManager == null)
        {
            return;
        }

        TaskData currentTask = taskManager.GetCurrentTask();

        if (currentTask == null || currentTask.taskId != fuseTaskId)
        {
            return;
        }

        taskResolvedByPanel = true;
        taskManager.ResolveCurrentTask(DecisionType.Authorize);
    }

    // Cambia el texto del panel segun el paso actual de la tarea.
    private void UpdateVisualFeedback()
    {
        if (statusText == null)
        {
            return;
        }

        if (!CanUsePanelNow() && !replacementInstalled)
        {
            statusText.text = inactiveText;
            statusText.color = normalTextColor;
            return;
        }

        if (replacementInstalled)
        {
            statusText.text = completedText;
            statusText.color = completedTextColor;
            return;
        }

        statusText.text = oldFuseRemoved ? waitingReplacementText : waitingOldFuseText;
        statusText.color = normalTextColor;
    }
}
