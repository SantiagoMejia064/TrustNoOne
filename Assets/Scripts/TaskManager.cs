using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

/// <summary>
/// Tipo de decision que puede tomar el jugador sobre una tarea.
/// </summary>
public enum DecisionType
{
    Authorize,
    Reject
}

/// <summary>
/// Define si una tarea es una orden legitima o un intento de sabotaje.
/// Esto es lo que el TaskManager usa para saber que decision es "correcta".
/// </summary>
public enum TaskType
{
    Legitimate,
    Sabotage
}

/// <summary>
/// Datos de una tarea individual. Se configura en el Inspector,
/// una entrada por cada una de las 8 tareas de la partida, en orden.
/// </summary>
[System.Serializable]
public class TaskData
{
    [SerializeField] private string id;
    [SerializeField, TextArea] private string message;
    [SerializeField] private TaskType type;

    public string Id => id;
    public string Message => message;
    public TaskType Type => type;
}

/// <summary>
/// Controla el flujo lineal de las 8 tareas: cuenta cuantas legitimas
/// se autorizaron/rechazaron y cuantas de sabotaje se autorizaron/rechazaron,
/// y dispara Game Over o Victoria segun corresponda.
/// </summary>
public class TaskManager : MonoBehaviour
{
    [Header("Tareas (en orden, lineal)")]
    [SerializeField] private List<TaskData> tasks = new List<TaskData>();

    [Header("Interfaz")]
    [SerializeField] private TextMeshProUGUI taskText;

    [Header("Condiciones de derrota")]
    [SerializeField] private int maxLegitimateRejected = 4;
    [SerializeField] private int maxSabotageAuthorized = 4;

    [Header("Escenas")]
    [SerializeField] private string victorySceneName = "Victoria";
    [SerializeField] private string gameOverSceneName = "GameOver";

    [Header("Eventos")]
    public UnityEvent<TaskData> onNewTaskShown;
    public UnityEvent<TaskData, DecisionType> onTaskResolved;

    private int currentTaskIndex = -1;

    // --- Checklist interno (el jugador no lo ve) ---
    [SerializeField] private int legitimateAuthorizedCount; // buenas hechas bien
    [SerializeField] private int legitimateRejectedCount;   // buenas rechazadas (cuenta para derrota)
    [SerializeField] private int sabotageAuthorizedCount;   // malas aceptadas (cuenta para derrota)
    [SerializeField] private int sabotageRejectedCount;     // malas rechazadas bien

    private bool runFinished;

    public TaskData CurrentTask =>
        currentTaskIndex >= 0 && currentTaskIndex < tasks.Count ? tasks[currentTaskIndex] : null;

    // --- Getters publicos del checklist, por si la UI/debug los necesita ---
    public int LegitimateAuthorizedCount => legitimateAuthorizedCount;
    public int LegitimateRejectedCount => legitimateRejectedCount;
    public int SabotageAuthorizedCount => sabotageAuthorizedCount;
    public int SabotageRejectedCount => sabotageRejectedCount;

    private void Start()
    {
        StartRun();
    }

    public void StartRun()
    {
        currentTaskIndex = -1;
        legitimateAuthorizedCount = 0;
        legitimateRejectedCount = 0;
        sabotageAuthorizedCount = 0;
        sabotageRejectedCount = 0;
        runFinished = false;

        ShowNextTask();
    }

    public void ShowNextTask()
    {
        if (runFinished)
        {
            return;
        }

        currentTaskIndex++;

        if (currentTaskIndex >= tasks.Count)
        {
            // Se completaron todas las tareas sin llegar a una condicion de derrota.
            TriggerVictory();
            return;
        }

        taskText.text = CurrentTask.Message;
        onNewTaskShown?.Invoke(CurrentTask);
    }

    public void ResolveCurrentTask(DecisionType decision)
    {
        if (runFinished || CurrentTask == null)
        {
            return;
        }

        TaskData resolvedTask = CurrentTask;

        RegisterDecision(resolvedTask, decision);
        Debug.Log("Tarea " + resolvedTask.Id + " - tipo: " + resolvedTask.Type + " - decision: " + decision);
        onTaskResolved?.Invoke(resolvedTask, decision);

        if (CheckDefeatConditions())
        {
            return;
        }

        // La tarea cambia a la siguiente inmediatamente despues de resolver,
        // como pide el dise�o (autorizar/rechazar -> consecuencia -> siguiente tarea).
        ShowNextTask();
    }

    private void RegisterDecision(TaskData task, DecisionType decision)
    {
        if (task.Type == TaskType.Legitimate)
        {
            if (decision == DecisionType.Authorize)
            {
                legitimateAuthorizedCount++;
            }
            else
            {
                legitimateRejectedCount++;
            }
        }
        else // TaskType.Sabotage
        {
            if (decision == DecisionType.Authorize)
            {
                sabotageAuthorizedCount++;
            }
            else
            {
                sabotageRejectedCount++;
            }
        }
    }

    private bool CheckDefeatConditions()
    {
        if (legitimateRejectedCount >= maxLegitimateRejected)
        {
            TriggerGameOver();
            return true;
        }

        if (sabotageAuthorizedCount >= maxSabotageAuthorized)
        {
            TriggerGameOver();
            return true;
        }

        return false;
    }

    private void TriggerVictory()
    {
        runFinished = true;
        UnitySceneManager.LoadScene(victorySceneName);
    }

    private void TriggerGameOver()
    {
        runFinished = true;
        UnitySceneManager.LoadScene(gameOverSceneName);
    }
}
