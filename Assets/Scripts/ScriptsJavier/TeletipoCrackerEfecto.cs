using TMPro;
using UnityEngine;

/// <summary>
/// Si las comunicaciones fueron comprometidas (tarea 3 autorizada por
/// error), simula que un "cracker" altero el siguiente mensaje del
/// teletipo, mostrando un mensaje falso en la tarea 4 en vez del real.
/// El jugador debe decidir sin saber que el mensaje es falso.
/// </summary>
public class TeletipoCrackerEfecto : MonoBehaviour
{
    [Header("Debe coincidir EXACTO con el Id de la tarea afectada (Tarea4)")]
    [SerializeField] private string taskId = "Tarea4";

    [SerializeField] private TaskManager taskManager;

    [Header("Debe ser la MISMA referencia que 'Task Text' del TaskManager")]
    [SerializeField] private TextMeshProUGUI taskText;

    [SerializeField, TextArea]
    private string mensajeFalso = "The enemy is trying to access the bunker, do not turn off the alarm.";

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
        Debug.Log($"[TeletipoCrackerEfecto] Tarea mostrada: '{task.Id}' | Esperando: '{taskId}' | Comprometidas: {EstadoPartida.ComunicacionesComprometidas}");

        if (task.Id != taskId)
        {
            return;
        }

        if (!EstadoPartida.ComunicacionesComprometidas)
        {
            return; // Comunicaciones intactas: se muestra el mensaje real, sin tocar nada.
        }

        if (taskText != null)
        {
            taskText.text = mensajeFalso;
        }

        Debug.Log("[Cracker] Mensaje de Tarea4 alterado por comunicaciones comprometidas.");
    }
}
