using System.Collections;
using UnityEngine;

public class VentilationController : MonoBehaviour, IInteractable
{
    [Header("Tarea")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private string ventilationTaskId = "detener_ventilacion_contaminada";

    [Header("Ventilacion")]
    [SerializeField] private bool ventilationRunning = true;
    [SerializeField] private float requiredStopDuration = 60f;
    [SerializeField] private string stopText = "E - DETENER VENTILACION";
    [SerializeField] private string startText = "E - ACTIVAR VENTILACION";

    private Coroutine stopTimerCoroutine;
    private bool resolvingAfterCompletedStop;

    private void OnEnable()
    {
        if (taskManager == null)
        {
            return;
        }

        taskManager.TaskShown += HandleTaskShown;
        taskManager.TaskResolved += HandleTaskResolved;
    }

    private void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.TaskShown -= HandleTaskShown;
            taskManager.TaskResolved -= HandleTaskResolved;
        }

        CancelStopTimer();
    }

    public string GetInteractionText()
    {
        return ventilationRunning ? stopText : startText;
    }

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        ventilationRunning = !ventilationRunning;
        Debug.Log("Ventilacion: " + (ventilationRunning ? "ACTIVA" : "DETENIDA"));

        if (ventilationRunning)
        {
            CancelStopTimer();
            return;
        }

        TryStartStopTimer();
    }

    private void HandleTaskShown(TaskData task)
    {
        if (task != null && task.taskId == ventilationTaskId && !ventilationRunning)
        {
            TryStartStopTimer();
        }
    }

    private void HandleTaskResolved(TaskData task, bool wasCorrect)
    {
        if (task == null || task.taskId != ventilationTaskId)
        {
            return;
        }

        CancelStopTimer();

        if (!resolvingAfterCompletedStop)
        {
            ventilationRunning = true;
            Debug.Log("La orden fue rechazada. La ventilacion permanece activa.");
        }

        resolvingAfterCompletedStop = false;
    }

    private void TryStartStopTimer()
    {
        TaskData currentTask = taskManager != null ? taskManager.GetCurrentTask() : null;

        if (currentTask == null
            || currentTask.taskId != ventilationTaskId
            || taskManager.IsCurrentTaskResolved())
        {
            return;
        }

        CancelStopTimer();
        stopTimerCoroutine = StartCoroutine(CompleteRequiredStop());
        Debug.Log("Ventilacion detenida. Iniciando conteo de " + requiredStopDuration + " segundos.");
    }

    private IEnumerator CompleteRequiredStop()
    {
        yield return new WaitForSeconds(Mathf.Max(0.1f, requiredStopDuration));
        stopTimerCoroutine = null;

        if (ventilationRunning || taskManager == null || taskManager.IsCurrentTaskResolved())
        {
            yield break;
        }

        TaskData currentTask = taskManager.GetCurrentTask();

        if (currentTask == null || currentTask.taskId != ventilationTaskId)
        {
            yield break;
        }

        resolvingAfterCompletedStop = true;
        Debug.Log("La ventilacion permanecio detenida durante el tiempo requerido. Enviando AUTORIZAR.");
        taskManager.ResolveCurrentTask(DecisionType.Authorize);
    }

    private void CancelStopTimer()
    {
        if (stopTimerCoroutine == null)
        {
            return;
        }

        StopCoroutine(stopTimerCoroutine);
        stopTimerCoroutine = null;
    }
}
