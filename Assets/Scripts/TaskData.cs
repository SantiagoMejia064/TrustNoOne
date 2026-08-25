using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTaskData", menuName = "Trust No One/Task Data")]
public class TaskData : ScriptableObject
{
    [Header("Identidad")]
    public string taskId;
    public string title;

    [Header("Mensaje")]
    [TextArea] public string message;
    [TextArea] public string howToVerify;

    [Header("Seleccion")]
    public TaskDifficulty difficulty;
    public bool canAppearRandomly = true;

    [Header("Condicion de verdad")]
    public TaskConditionType truthCondition;
    public DecisionType decisionWhenConditionIsTrue;
    public DecisionType decisionWhenConditionIsFalse;

    [Header("Impacto principal si el jugador acierta")]
    public BunkerSystemType successAffectedSystem;
    public int successSystemDelta;

    [Header("Impacto principal si el jugador falla")]
    public BunkerSystemType failureAffectedSystem;
    public int failureSystemDelta;

    [Header("Efectos secundarios")]
    public List<TaskEffectType> successEffects = new List<TaskEffectType>();
    public List<TaskEffectType> failureEffects = new List<TaskEffectType>();

    public DecisionType GetCorrectDecision(BunkerStateManager bunkerStateManager)
    {
        if (bunkerStateManager == null)
        {
            Debug.LogWarning("TaskData no puede evaluar la decision correcta porque falta BunkerStateManager.");
            return decisionWhenConditionIsFalse;
        }

        bool conditionResult = bunkerStateManager.CheckCondition(truthCondition);
        return conditionResult ? decisionWhenConditionIsTrue : decisionWhenConditionIsFalse;
    }
}
