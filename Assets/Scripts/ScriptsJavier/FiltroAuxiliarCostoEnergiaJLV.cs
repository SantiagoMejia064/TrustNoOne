using UnityEngine;


public class FiltroAuxiliarSwitchJLV : MonoBehaviour, IInteractable
{
    [Header("Referencias")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private BunkerStateManager bunkerStateManager;

    [Header("Identidad de la tarea")]
    [Tooltip("Debe coincidir EXACTAMENTE con el taskId del TaskData del caso 9.")]
    [SerializeField] private string targetTaskId = "encender_filtro_auxiliar";

    [Header("Texto")]
    [SerializeField] private string interactionText = "E - Activar filtro auxiliar";

    [Header("Consecuencias al activar")]
    [SerializeField] private int oxigenoDelta = 1;
    [SerializeField] private int energiaDelta = -1;

    [Header("Estado")]
    [SerializeField] private bool desbloqueado = false;
    [SerializeField] private bool activado = false;

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
            Debug.Log("[Caso 9] Filtro auxiliar desbloqueado. Ya se puede activar fisicamente.");
        }
        else
        {
            Debug.Log("[Caso 9] Orden rechazada. El filtro no se activa; el problema de oxigeno empeora.");
        }
    }

    public string GetInteractionText()
    {
        return interactionText;
    }

    public bool CanInteract()
    {
        return desbloqueado && !activado;
    }

    public void Interact()
    {
        if (!CanInteract())
        {
            return;
        }

        activado = true;

        if (bunkerStateManager != null)
        {
            bunkerStateManager.ModifySystemState(BunkerSystemType.Oxygen, oxigenoDelta);
            bunkerStateManager.ModifySystemState(BunkerSystemType.Energy, energiaDelta);
        }

        Debug.Log("[Caso 9] Filtro auxiliar activado. Oxigeno mejora, Energia se consume.");

        // SUPUESTO A CONFIRMAR CON EL EQUIPO (igual que en el caso 8):
        // no sabemos si otro sistema ya llama ShowNextTask() en otro
        // punto del flujo. Se llama aca para no dejar el caso trabado.
        if (taskManager != null)
        {
            taskManager.ShowNextTask();
        }
    }
}
