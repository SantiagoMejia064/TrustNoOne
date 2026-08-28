using UnityEngine;

public class BotonRechazar : MonoBehaviour, IInteractable
{
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private string manualTaskId = "";
    [SerializeField] private string activeInteractionText = "E - Reject";
    [SerializeField] private string inactiveInteractionText = "You did your homework, so you can't back out now";
    [SerializeField] private string interactionText = "E - Reject";
    [SerializeField] private string interactableName = "Rejected button";
    [SerializeField] private bool canInteract = true;

    public FiltroAuxiliar filtroAuxiliar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Activar();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public string GetInteractionText()
    {
        return interactionText;
    }

    public bool CanInteract()
    {
        return canInteract;
    }

    public void Interact()
    {
        Debug.Log("Interactuando con: " + interactableName);

        if (taskManager == null)
        {
            Debug.LogWarning("BotonRechazar: taskManager no esta asignado.");
            return;
        }

        if (taskManager.CurrentTask == null)
        {
            Debug.LogWarning("BotonRechazar: no hay tarea actual.");
            return;
        }

        if (taskManager.CurrentTask.Id == manualTaskId)
        {
            filtroAuxiliar?.ActivarGases();
        }

        taskManager?.ResolveCurrentTask(DecisionType.Reject);
    }

    public void Activar()
    {
        interactionText = activeInteractionText;
        canInteract = true;
    }

    public void Desactivar()
    {
        interactionText = inactiveInteractionText;
        canInteract = false;
    }
}
