using UnityEngine;

/// <summary>
/// Al aparecer la tarea "Desactivar alarma de oxigeno":
/// - Deja el boton Autorizar desactivado (el jugador debe apagar la
///   alarma primero, interactuando con AlarmaOxigenoInteractable).
/// - Llama a Activar() en la alarma para que empiece a parpadear/sonar
///   y se pueda interactuar con ella.
///
/// Ademas, al resolverse esta tarea en particular, muestra por consola
/// el estado del checklist interno del TaskManager (solo para debug,
/// ya que el jugador no debe ver estos numeros en el juego real).
/// </summary>
public class TareaAlarmaOxigenoSetup : MonoBehaviour
{
    [Header("Debe coincidir EXACTO con el Id de esta tarea en el TaskManager")]
    [SerializeField] private string taskId = "Tarea4";

    [SerializeField] private TaskManager taskManager;
    [SerializeField] private BotonAutorizar botonAutorizar;
    [SerializeField] private AlarmaOxigenoInteractable alarmaInteractable;

    private void OnEnable()
    {
        if (taskManager != null)
        {
            taskManager.onNewTaskShown.AddListener(OnNewTaskShown);
            taskManager.onTaskResolved.AddListener(OnTaskResolved);
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
        Debug.Log($"[TareaAlarmaOxigenoSetup] Tarea mostrada: '{task.Id}' | Esperando: '{taskId}'");

        if (task.Id != taskId)
        {
            return;
        }

        if (botonAutorizar != null)
        {
            botonAutorizar.Desactivar();
        }

        if (alarmaInteractable != null)
        {
            alarmaInteractable.Activar();
        }
    }

    private void OnTaskResolved(TaskData task, DecisionType decision)
    {
        if (task.Id != taskId)
        {
            return;
        }

        if (decision == DecisionType.Authorize)
        {
            Debug.Log("Correcto: alarma de oxigeno desactivada.");
        }
        else
        {
            Debug.Log("Error: se rechazo la desactivacion. La alarma suena y parpadea por el resto de la partida.");

            if (alarmaInteractable != null)
            {
                alarmaInteractable.Bloquear();
            }
        }

        Debug.Log(
            $"[Checklist] Legitimas autorizadas: {taskManager.LegitimateAuthorizedCount} | " +
            $"Legitimas rechazadas: {taskManager.LegitimateRejectedCount} | " +
            $"Sabotajes autorizados: {taskManager.SabotageAuthorizedCount} | " +
            $"Sabotajes rechazados: {taskManager.SabotageRejectedCount}"
        );
    }
}