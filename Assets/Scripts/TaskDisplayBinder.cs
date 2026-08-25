using UnityEngine;

public class TaskDisplayBinder : MonoBehaviour
{
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private TeletypeController teletypeController;

    private void OnEnable()
    {
        if (taskManager == null)
        {
            Debug.LogWarning("Falta asignar TaskManager en TaskDisplayBinder.");
            return;
        }

        taskManager.TaskShown += HandleTaskShown;
        taskManager.onRunCompleted.AddListener(HandleRunCompleted);
    }

    private void OnDisable()
    {
        if (taskManager == null)
        {
            return;
        }

        taskManager.TaskShown -= HandleTaskShown;
        taskManager.onRunCompleted.RemoveListener(HandleRunCompleted);
    }

    private void HandleTaskShown(TaskData task)
    {
        if (teletypeController == null)
        {
            Debug.LogWarning("Falta asignar TeletypeController en TaskDisplayBinder.");
            return;
        }

        teletypeController.ShowTask(task);
    }

    private void HandleRunCompleted()
    {
        if (teletypeController != null)
        {
            teletypeController.Clear();
        }
    }
}
