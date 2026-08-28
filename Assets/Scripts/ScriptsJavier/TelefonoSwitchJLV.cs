using UnityEngine;


public class TelefonoSwitchJLV : MonoBehaviour, IInteractable
{
    [Header("Referencias")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private TelefonoJLV telefono;

    [Header("Identidad de la tarea")]
    [Tooltip("Debe coincidir EXACTAMENTE con el taskId del TaskData del caso 12.")]
    [SerializeField] private string targetTaskId = "reactivar_linea_telefonica";

    [Header("Texto")]
    [SerializeField] private string interactionText = "E - Reiniciar linea telefonica";

    [Header("Estado")]
    [SerializeField] private bool desbloqueado = false;
    [SerializeField] private bool reiniciado = false;

    private void OnEnable()
    {
        if (taskManager != null)
        {
            taskManager.TaskResolved += HandleTaskResolved;
        }
    }

    private void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.TaskResolved -= HandleTaskResolved;
        }
    }

    private void HandleTaskResolved(TaskData task, bool wasCorrect)
    {
        if (task == null || task.taskId != targetTaskId)
        {
            return;
        }

        if (wasCorrect)
        {
            desbloqueado = true;
            Debug.Log("[Caso 12] Telefono desbloqueado. Ya se puede reiniciar fisicamente.");
        }
        else
        {
            Debug.Log("[Caso 12] Orden rechazada. La linea sigue perdida.");
        }
    }

    public string GetInteractionText()
    {
        return interactionText;
    }

    public bool CanInteract()
    {
        return desbloqueado && !reiniciado;
    }

    public void Interact()
    {
        if (!CanInteract())
        {
            return;
        }

        reiniciado = true;

        if (telefono != null)
        {
            telefono.SetLineaActiva(true);
        }

        Debug.Log("[Caso 12] Linea telefonica reiniciada.");

        // SUPUESTO A CONFIRMAR CON EL EQUIPO (igual que en los casos
        // 8 y 9): no sabemos si otro sistema ya llama ShowNextTask()
        // en otro punto del flujo.
        if (taskManager != null)
        {
            taskManager.ShowNextTask();
        }
    }
}