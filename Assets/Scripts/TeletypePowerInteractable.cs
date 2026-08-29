using UnityEngine;

public class TeletypePowerInteractable : MonoBehaviour, IInteractable
{
    [Header("Task")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private string taskId = "Tarea8";
    [SerializeField] private BotonAutorizar botonAutorizar;
    [SerializeField] private BotonRechazar botonRechazar;

    [Header("Interaction")]
    [SerializeField] private string interactionText = "E - Disconnect teletype";
    [SerializeField] private string interactableName = "Teletype power";

    private bool taskDone;

    // Entrega el texto que aparece al mirar el boton del teletipo.
    public string GetInteractionText()
    {
        return interactionText;
    }

    // Permite usar el boton solo durante la Tarea8 y una sola vez.
    public bool CanInteract()
    {
        return !taskDone
            && taskManager != null
            && taskManager.CurrentTask != null
            && taskManager.CurrentTask.Id == taskId;
    }

    // Marca la accion como hecha y habilita Autorizar.
    public void Interact()
    {
        if (!CanInteract())
        {
            return;
        }

        Debug.Log("Interactuando con: " + interactableName);

        taskDone = true;
        botonAutorizar?.Activar();
        botonRechazar?.Desactivar();
    }
}