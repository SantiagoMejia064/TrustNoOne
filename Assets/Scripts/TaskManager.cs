using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TaskManager : MonoBehaviour
{
    [Header("Tareas")]
    [SerializeField] private List<TaskData> allTasks = new List<TaskData>();
    [SerializeField] private int minTasksPerRun = 8;
    [SerializeField] private int maxTasksPerRun = 12;
    [SerializeField] private BunkerStateManager bunkerStateManager;

    [Header("Eventos")]
    public UnityEvent onRunStarted = new UnityEvent();
    public UnityEvent onTaskShown = new UnityEvent();
    public UnityEvent onCorrectDecision = new UnityEvent();
    public UnityEvent onWrongDecision = new UnityEvent();
    public UnityEvent onRunCompleted = new UnityEvent();

    public event Action<TaskData> TaskShown;
    public event Action<TaskData, bool> TaskResolved;
    public event Action<List<TaskEffectType>> EffectsTriggered;

    private readonly List<TaskData> selectedTasks = new List<TaskData>();
    private int currentTaskIndex = -1;
    private bool currentTaskResolved;

    public void StartRun()
    {
        selectedTasks.Clear();
        currentTaskIndex = -1;
        currentTaskResolved = false;

        List<TaskData> availableTasks = allTasks.FindAll(task => task != null && task.canAppearRandomly);
        int minCount = Mathf.Max(0, Mathf.Min(minTasksPerRun, maxTasksPerRun));
        int maxCount = Mathf.Max(minCount, Mathf.Max(minTasksPerRun, maxTasksPerRun));
        int requestedTaskCount = UnityEngine.Random.Range(minCount, maxCount + 1);

        if (availableTasks.Count < requestedTaskCount)
        {
            Debug.LogWarning("Hay menos tareas disponibles que las solicitadas. Se usaran todas las tareas disponibles.");
            requestedTaskCount = availableTasks.Count;
        }

        for (int i = 0; i < requestedTaskCount; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableTasks.Count);
            selectedTasks.Add(availableTasks[randomIndex]);
            availableTasks.RemoveAt(randomIndex);
        }

        onRunStarted?.Invoke();
        ShowNextTask();
    }

    public void ShowNextTask()
    {
        currentTaskIndex++;

        if (currentTaskIndex >= selectedTasks.Count)
        {
            CompleteRun();
            return;
        }

        TaskData currentTask = GetCurrentTask();
        currentTaskResolved = false;
        onTaskShown?.Invoke();
        TaskShown?.Invoke(currentTask);
    }

    public void ResolveCurrentTask(DecisionType playerDecision)
    {
        TaskData currentTask = GetCurrentTask();

        if (currentTask == null)
        {
            Debug.LogWarning("No hay una tarea actual para resolver.");
            return;
        }

        if (currentTaskResolved)
        {
            Debug.LogWarning("La tarea actual ya fue resuelta.");
            return;
        }

        if (bunkerStateManager == null)
        {
            Debug.LogWarning("TaskManager no puede resolver la tarea porque falta BunkerStateManager.");
            return;
        }

        DecisionType correctDecision = currentTask.GetCorrectDecision(bunkerStateManager);
        bool wasCorrect = playerDecision == correctDecision;
        currentTaskResolved = true;

        Debug.Log(
            "Tarea: " + currentTask.title
            + " | Decision elegida: " + playerDecision
            + " | Decision correcta: " + correctDecision
            + " | Resultado: " + (wasCorrect ? "CORRECTA" : "INCORRECTA"));

        if (wasCorrect)
        {
            bunkerStateManager.ModifySystemState(currentTask.successAffectedSystem, currentTask.successSystemDelta);
            onCorrectDecision?.Invoke();
            EffectsTriggered?.Invoke(currentTask.successEffects);
        }
        else
        {
            bunkerStateManager.ModifySystemState(currentTask.failureAffectedSystem, currentTask.failureSystemDelta);
            onWrongDecision?.Invoke();
            EffectsTriggered?.Invoke(currentTask.failureEffects);
        }

        TaskResolved?.Invoke(currentTask, wasCorrect);
    }

    public TaskData GetCurrentTask()
    {
        if (currentTaskIndex < 0 || currentTaskIndex >= selectedTasks.Count)
        {
            return null;
        }

        return selectedTasks[currentTaskIndex];
    }

    public int GetCurrentTaskIndex()
    {
        return currentTaskIndex;
    }

    public int GetSelectedTaskCount()
    {
        return selectedTasks.Count;
    }

    public bool IsCurrentTaskResolved()
    {
        return currentTaskResolved;
    }

    private void CompleteRun()
    {
        onRunCompleted?.Invoke();
        Debug.Log("La partida de tareas termino.");
    }
}
