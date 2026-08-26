using System.Collections.Generic;
using UnityEngine;

public class GeneratorPowerController : MonoBehaviour, IInteractable
{
    [Header("Tarea")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private string generatorTaskId = "aumentar_potencia_generador";

    [Header("Generador")]
    [SerializeField] private bool generatorActive = true;
    [SerializeField] private int currentPowerPercent = 100;
    [SerializeField] private string interactionText = "E - AUMENTAR GENERADOR AL 120%";

    [Header("Luces que se queman")]
    [SerializeField] private Light[] lightsToBurn = new Light[6];

    [Header("Oscuridad del ambiente")]
    [SerializeField] private float failureFogDensity = 0.2f;
    [SerializeField] private float failureAmbientIntensity = 0f;

    // Escucha efectos de tareas para reaccionar si alguna quema luces o exige linterna.
    private void OnEnable()
    {
        if (taskManager != null)
        {
            taskManager.EffectsTriggered += HandleEffects;
        }
    }

    // Deja de escuchar efectos cuando el generador ya no esta activo.
    private void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.EffectsTriggered -= HandleEffects;
        }
    }

    // Devuelve el texto que aparece al mirar el generador.
    public string GetInteractionText()
    {
        return interactionText;
    }

    // Permite usar el generador solo durante su tarea y si aun no llego al 120%.
    public bool CanInteract()
    {
        TaskData currentTask = taskManager != null ? taskManager.GetCurrentTask() : null;

        return generatorActive
            && currentPowerPercent < 120
            && currentTask != null
            && currentTask.taskId == generatorTaskId
            && !taskManager.IsCurrentTaskResolved();
    }

    // Aumenta el generador y resuelve la tarea como autorizar.
    public void Interact()
    {
        if (!CanInteract())
        {
            Debug.LogWarning("El generador no puede aumentar su potencia en este momento.");
            return;
        }

        currentPowerPercent = 120;
        Debug.Log("Generador aumentado al 120%. Enviando AUTORIZAR.");
        taskManager.ResolveCurrentTask(DecisionType.Authorize);
    }

    // Aplica consecuencias secundarias recibidas desde TaskManager.
    private void HandleEffects(List<TaskEffectType> effects)
    {
        if (effects == null)
        {
            return;
        }

        if (effects.Contains(TaskEffectType.BurnLights))
        {
            BurnAssignedLights();
        }

        if (effects.Contains(TaskEffectType.RequireFlashlight))
        {
            Debug.Log("La habitacion quedo parcialmente oscura. Se requiere una linterna.");
        }
    }

    // Apaga las luces asignadas y oscurece el ambiente para representar la falla.
    private void BurnAssignedLights()
    {
        int burnedLights = 0;

        foreach (Light assignedLight in lightsToBurn)
        {
            if (assignedLight == null)
            {
                continue;
            }

            assignedLight.enabled = false;
            burnedLights++;
        }

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = failureFogDensity;
        RenderSettings.ambientIntensity = failureAmbientIntensity;

        Debug.Log("Luces quemadas por sobrecarga: " + burnedLights);
        Debug.Log("Ambiente oscurecido. Fog Density: " + failureFogDensity
            + " | Environment Intensity Multiplier: " + failureAmbientIntensity);
    }
}
