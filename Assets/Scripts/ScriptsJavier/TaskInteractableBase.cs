using UnityEngine;


public abstract class TaskInteractableBase : MonoBehaviour, IInteractable
{
    [Header("Task gating")]
    [SerializeField] private string taskId;
    [SerializeField] private TaskManager taskManager;

    [Header("Interaccion")]
    [SerializeField] private string interactionText = "E - Interactuar";
    [SerializeField] private string interactableName = "Objeto de tarea";

    [Header("Botones asociados")]
    [SerializeField] private BotonAutorizar botonAutorizar;
    [SerializeField] private BotonRechazar botonRechazar;

    [Header("Comportamiento")]
    [SerializeField] private bool desactivarAutorizarAlInicio = true;
    [SerializeField] private bool unicaInteraccion = true;

    private bool tareaActiva;
    private bool canInteract;

    protected virtual void OnEnable()
    {
        if (taskManager != null)
        {
            taskManager.onNewTaskShown.AddListener(HandleNewTaskShown);
            taskManager.onTaskResolved.AddListener(HandleTaskResolved);
        }
    }

    protected virtual void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.onNewTaskShown.RemoveListener(HandleNewTaskShown);
            taskManager.onTaskResolved.RemoveListener(HandleTaskResolved);
        }
    }

    private void HandleNewTaskShown(TaskData task)
    {
        if (task.Id != taskId)
        {
            return;
        }

        tareaActiva = true;
        canInteract = true;

        if (desactivarAutorizarAlInicio && botonAutorizar != null)
        {
            botonAutorizar.Desactivar();
        }

        OnTaskActivada();
    }

    private void HandleTaskResolved(TaskData task, DecisionType decision)
    {
        if (task.Id != taskId)
        {
            return;
        }

        // Se resolvio esta tarea (autorizada o rechazada): el objeto ya
        // no debe responder a mas interacciones, pase lo que pase despues.
        tareaActiva = false;
        canInteract = false;

        OnTaskResuelta(decision);
    }

    public string GetInteractionText() => interactionText;

    public bool CanInteract() => tareaActiva && canInteract;

    public void Interact()
    {
        if (!tareaActiva || !canInteract)
        {
            return;
        }

        Debug.Log("Interactuando con: " + interactableName);

        EjecutarEfecto();

        if (botonAutorizar != null)
        {
            botonAutorizar.Activar();
        }

        if (botonRechazar != null)
        {
            botonRechazar.Desactivar();
        }

        if (unicaInteraccion)
        {
            canInteract = false;
        }
    }

    /// <summary>Efecto propio de esta tarea (sonido, animacion, color, etc.).</summary>
    protected abstract void EjecutarEfecto();

    /// <summary>Hook opcional: se llama cuando aparece esta tarea.</summary>
    protected virtual void OnTaskActivada() { }

    /// <summary>Hook opcional: se llama cuando se resuelve esta tarea.</summary>
    protected virtual void OnTaskResuelta(DecisionType decision) { }
}
