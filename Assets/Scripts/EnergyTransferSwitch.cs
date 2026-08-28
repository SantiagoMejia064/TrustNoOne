using TMPro;
using UnityEngine;

public class EnergyTransferSwitch : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private BunkerStateManager bunkerStateManager;

    [Header("Task")]
    [SerializeField] private string energyTransferTaskId = "transferir_energia_ventilacion";
    [SerializeField] private bool requireTaskActiveToInteract = true;
    [SerializeField] private bool resolveTaskWhenTransferred = true;

    [Header("State")]
    [SerializeField] private bool transferDone;

    [Header("Interaction Text")]
    [SerializeField] private string transferText = "E - TRANSFERIR ENERGIA";

    [Header("Visuals")]
    [SerializeField] private Transform switchHandle;
    [SerializeField] private Vector3 idleLocalEulerAngles;
    [SerializeField] private Vector3 transferredLocalEulerAngles = new Vector3(-45f, 0f, 0f);
    [SerializeField] private GameObject idleVisuals;
    [SerializeField] private GameObject transferredVisuals;

    [Header("Panel Feedback")]
    [SerializeField] private TMP_Text oxygenStateText;
    [SerializeField] private TMP_Text communicationsStateText;

    // Escucha cambios de tarea y sistemas para refrescar el panel.
    private void OnEnable()
    {
        if (taskManager != null)
        {
            taskManager.TaskShown += HandleTaskShown;
        }

        if (bunkerStateManager != null)
        {
            bunkerStateManager.onOxygenChanged.AddListener(UpdateVisualFeedback);
            bunkerStateManager.onCommunicationsChanged.AddListener(UpdateVisualFeedback);
        }
    }

    // Quita las suscripciones cuando el objeto se desactiva.
    private void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.TaskShown -= HandleTaskShown;
        }

        if (bunkerStateManager != null)
        {
            bunkerStateManager.onOxygenChanged.RemoveListener(UpdateVisualFeedback);
            bunkerStateManager.onCommunicationsChanged.RemoveListener(UpdateVisualFeedback);
        }
    }

    // Aplica el estado visual inicial del switch.
    private void Start()
    {
        UpdateVisualFeedback();
    }

    // Devuelve el texto que se muestra al mirar el switch.
    public string GetInteractionText()
    {
        return transferText;
    }

    // Permite usar el switch solo una vez y, si se pide, solo durante su tarea.
    public bool CanInteract()
    {
        return !transferDone && CanUseTransferNow();
    }

    // Marca la palanca como usada y reporta la decision al TaskManager.
    public void Interact()
    {
        if (!CanInteract())
        {
            Debug.LogWarning("La transferencia de energia no se puede usar en este momento.");
            return;
        }

        transferDone = true;
        UpdateVisualFeedback();
        TryResolveEnergyTransferTask();
    }

    // Reinicia el switch cuando esta tarea aparece de nuevo.
    private void HandleTaskShown(TaskData task)
    {
        if (task != null && task.taskId == energyTransferTaskId)
        {
            transferDone = false;
        }

        UpdateVisualFeedback();
    }

    // Si la tarea activa es esta transferencia, usar la palanca cuenta como autorizar.
    private void TryResolveEnergyTransferTask()
    {
        if (!resolveTaskWhenTransferred || taskManager == null)
        {
            return;
        }

        TaskData currentTask = taskManager.GetCurrentTask();

        if (currentTask == null || currentTask.taskId != energyTransferTaskId || taskManager.IsCurrentTaskResolved())
        {
            return;
        }

        taskManager.ResolveCurrentTask(DecisionType.Authorize);
    }

    // Verifica si el switch puede usarse segun la tarea actual.
    private bool CanUseTransferNow()
    {
        if (!requireTaskActiveToInteract)
        {
            return true;
        }

        TaskData currentTask = taskManager != null ? taskManager.GetCurrentTask() : null;
        return currentTask != null && currentTask.taskId == energyTransferTaskId && !taskManager.IsCurrentTaskResolved();
    }

    // Actualiza palanca, luces opcionales y textos de estado.
    private void UpdateVisualFeedback()
    {
        if (switchHandle != null)
        {
            switchHandle.localRotation = Quaternion.Euler(transferDone ? transferredLocalEulerAngles : idleLocalEulerAngles);
        }

        if (idleVisuals != null)
        {
            idleVisuals.SetActive(!transferDone);
        }

        if (transferredVisuals != null)
        {
            transferredVisuals.SetActive(transferDone);
        }

        if (bunkerStateManager == null)
        {
            return;
        }

        if (oxygenStateText != null)
        {
            oxygenStateText.text = "OXIGENO: " + bunkerStateManager.GetSystemState(BunkerSystemType.Oxygen).ToString().ToUpperInvariant();
        }

        if (communicationsStateText != null)
        {
            communicationsStateText.text = "COMUNICACIONES: "
                + bunkerStateManager.GetSystemState(BunkerSystemType.Communications).ToString().ToUpperInvariant();
        }
    }
}