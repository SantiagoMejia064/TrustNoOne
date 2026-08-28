using UnityEngine;


public class RadioSwitchJLV : MonoBehaviour, IInteractable
{
    [Header("Referencias")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private BunkerStateManager bunkerStateManager;
    [SerializeField] private RadioJLV radio;

    [Header("Identidad de la tarea")]
    [Tooltip("Debe coincidir EXACTAMENTE con el taskId del TaskData del caso 11.")]
    [SerializeField] private string targetTaskId = "desconectar_radio";

    [Tooltip("Debe coincidir con el truthCondition configurado en el TaskData.")]
    [SerializeField] private TaskConditionType condicionDeVerdad = TaskConditionType.CommunicationsAreLowOrWorse;

    [Header("Texto")]
    [SerializeField] private string interactionText = "E - Desconectar receptor";

    [Header("Duracion del apagado (en tareas)")]
    [SerializeField] private int duracionMinima = 1;
    [SerializeField] private int duracionMaxima = 2;

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


        bool jugadorObedecio = condicionEraVerdadera == wasCorrect;

        if (jugadorObedecio)
        {
            desbloqueado = true;
            Debug.Log("[Caso 11] Receptor desbloqueado: el jugador eligio desconectarlo.");
        }
        else
        {
            Debug.Log("[Caso 11] El jugador no obedecio. La radio sigue conectada.");

            
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

        int duracion = Random.Range(duracionMinima, duracionMaxima + 1);

        if (radio != null)
        {
            radio.Desconectar(duracion);
        }

        Debug.Log("[Caso 11] Receptor desconectado fisicamente durante " + duracion + " tarea(s).");

        if (taskManager != null)
        {
            taskManager.ShowNextTask();
        }
    }
}