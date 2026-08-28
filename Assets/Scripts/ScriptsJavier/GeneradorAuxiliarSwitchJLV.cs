using UnityEngine;


public class GeneradorAuxiliarSwitchJLV : MonoBehaviour, IInteractable
{
    [Header("Referencias")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private BunkerStateManager bunkerStateManager;

    [Header("Identidad de la tarea")]
    [Tooltip("Debe coincidir EXACTAMENTE con el taskId del TaskData del caso 7.")]
    [SerializeField] private string targetTaskId = "cambiar_generador_auxiliar";

    [Tooltip("Debe coincidir con el truthCondition configurado en el TaskData.")]
    [SerializeField] private TaskConditionType condicionDeVerdad = TaskConditionType.AuxiliaryGeneratorIsAvailable;

    [Header("Texto")]
    [SerializeField] private string interactionText = "E - Cambiar al generador auxiliar";

    [Header("Apagon parcial si el auxiliar estaba averiado/descargado")]
    [SerializeField] private int energiaDeltaSiFalla = -1;

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

        // Misma logica que ventilacion/radio: obedecio si la
        // condicion y el acierto coinciden.
        bool jugadorObedecio = condicionEraVerdadera == wasCorrect;

        if (jugadorObedecio)
        {
            desbloqueado = true;
            Debug.Log("[Caso 7] Switch del generador auxiliar desbloqueado.");
        }
        else
        {
            Debug.Log("[Caso 7] El jugador no obedecio. No hay switch que activar.");

            // SUPUESTO A CONFIRMAR CON EL EQUIPO (igual que en los
            // casos anteriores): avanzamos aca directo si no hay
            // nada fisico que interactuar.
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

        bool auxiliarRealmenteDisponible = bunkerStateManager != null
            && bunkerStateManager.IsAuxiliaryGeneratorAvailable();

        if (!auxiliarRealmenteDisponible && bunkerStateManager != null)
        {
            // El auxiliar estaba averiado/descargado: apagon parcial
            // ademas del failureSystemDelta que ya aplico la consola.
            bunkerStateManager.ModifySystemState(BunkerSystemType.Energy, energiaDeltaSiFalla);
            Debug.Log("[Caso 7] Apagon parcial: el auxiliar no respondio correctamente.");
        }
        else
        {
            Debug.Log("[Caso 7] Cambio al generador auxiliar exitoso.");
        }

        if (taskManager != null)
        {
            taskManager.ShowNextTask();
        }
    }
}