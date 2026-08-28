using UnityEngine;


public class VentilacionSwitchJLV : MonoBehaviour, IInteractable
{
    [Header("Referencias")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private BunkerStateManager bunkerStateManager;

    [Header("Identidad de la tarea")]
    [Tooltip("Debe coincidir EXACTAMENTE con el taskId del TaskData del caso 10.")]
    [SerializeField] private string targetTaskId = "aumentar_ventilacion";

    [Tooltip("Debe coincidir con el truthCondition configurado en el TaskData.")]
    [SerializeField] private TaskConditionType condicionDeVerdad = TaskConditionType.OxygenIsLowOrWorse;

    [Header("Texto")]
    [SerializeField] private string interactionText = "E - Ventilacion al maximo";

    [Header("Costo de energia al obedecer")]
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

        bool condicionEraVerdadera = bunkerStateManager != null
            && bunkerStateManager.CheckCondition(condicionDeVerdad);

        // Si la condicion era verdadera, la decision correcta es
        // Authorize -> el jugador obedecio si wasCorrect es true.
        // Si la condicion era falsa, la decision correcta es Reject
        // -> el jugador obedecio (eligio Authorize) si wasCorrect es false.
        bool jugadorObedecio = condicionEraVerdadera == wasCorrect;

        if (jugadorObedecio)
        {
            desbloqueado = true;
            Debug.Log("[Caso 10] Ventilacion desbloqueada: el jugador eligio obedecer.");
        }
        else
        {
            Debug.Log("[Caso 10] El jugador no obedecio. No hay switch que activar.");

            // SUPUESTO A CONFIRMAR CON EL EQUIPO: si no obedece, no
            // hay nada fisico que interactuar, asi que avanzamos
            // aca directamente para no dejar el caso trabado.
            if (taskManager != null)
            {
                taskManager.ShowNextTask();
            }
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
            bunkerStateManager.ModifySystemState(BunkerSystemType.Energy, energiaDelta);
        }

        Debug.Log("[Caso 10] Ventilacion al maximo activada. Energia consumida.");

        // SUPUESTO A CONFIRMAR CON EL EQUIPO (igual que en los casos
        // 8, 9 y 12): no sabemos si otro sistema ya llama ShowNextTask().
        if (taskManager != null)
        {
            taskManager.ShowNextTask();
        }
    }
}