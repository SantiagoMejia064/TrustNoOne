using UnityEngine;

/// <summary>
/// Se engancha DESPUES de TaskButtonsResetter (debe ir mas abajo en la
/// lista del evento onNewTaskShown, en el Inspector) para dejar el boton
/// Autorizar desactivado al inicio de esta tarea especifica, ya que aqui
/// el jugador debe desconectar comunicaciones antes de poder autorizar.
/// </summary>
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
        }
    }

    private void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.onNewTaskShown.RemoveListener(OnNewTaskShown);
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
}