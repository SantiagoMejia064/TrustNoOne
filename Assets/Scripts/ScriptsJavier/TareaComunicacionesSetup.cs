using UnityEngine;


public class TareaComunicacionesSetup : MonoBehaviour
{
    [Header("Debe coincidir EXACTO con el Id de esta tarea en el TaskManager")]
    [SerializeField] private string taskId = "TR-COM-01";

    [SerializeField] private TaskManager taskManager;
    [SerializeField] private BotonAutorizar botonAutorizar;

    private void OnEnable()
    {
        if (taskManager != null)
        {
            taskManager.onNewTaskShown.AddListener(OnNewTaskShown);
            taskManager.onTaskResolved.AddListener(OnTaskResolved);   // <-- ¿esta línea está?
        }
    }
    private void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.onNewTaskShown.RemoveListener(OnNewTaskShown);
            taskManager.onTaskResolved.RemoveListener(OnTaskResolved);
        }
    }

    private void OnNewTaskShown(TaskData task)
    {
        if (task.Id != taskId)
        {
            return;
        }

        if (botonAutorizar != null)
        {
            botonAutorizar.Desactivar();
        }
    }

    private void OnTaskResolved(TaskData task, DecisionType decision)
    {
        Debug.Log($"[TareaComunicacionesSetup] Resuelta: '{task.Id}' | Esperando: '{taskId}' | Decision: {decision}");

        if (task.Id != taskId)
        {
            return;
        }

        if (decision == DecisionType.Authorize)
        {
            EstadoPartida.ComunicacionesComprometidas = true;
            Debug.Log("[Cracker] Comunicaciones comprometidas: el proximo mensaje del teletipo sera alterado.");
        }
    }
}