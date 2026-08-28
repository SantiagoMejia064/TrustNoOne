using UnityEngine;


public class AlarmaO2SonidoJLV : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private AudioSource audioAlarma;

    [Header("Identidad de la tarea")]
    [Tooltip("Debe coincidir EXACTAMENTE con el taskId del TaskData del caso 8.")]
    [SerializeField] private string targetTaskId = "alarma_o2_sabotaje";

    private void OnEnable()
    {
        if (taskManager != null)
        {
            taskManager.TaskShown += HandleTaskShown;
        }
    }

    private void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.TaskShown -= HandleTaskShown;
        }
    }

    private void HandleTaskShown(TaskData task)
    {
        if (task == null || task.taskId != targetTaskId)
        {
            return;
        }

        if (audioAlarma != null && audioAlarma.clip != null)
        {
            audioAlarma.loop = true;
            audioAlarma.Play();
        }
    }
}