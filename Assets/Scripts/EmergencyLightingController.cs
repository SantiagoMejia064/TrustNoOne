using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmergencyLightingController : MonoBehaviour, IInteractable
{
    [Header("Tarea")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private string emergencyTaskId = "activar_iluminacion_emergencia";
    [SerializeField] private string interactionText = "E - ACTIVAR ILUMINACION DE EMERGENCIA";

    [Header("Luces")]
    [SerializeField] private Light[] mainLights;
    [SerializeField] private Light[] emergencyLights;
    [SerializeField] private float warningBlinkInterval = 0.35f;
    [SerializeField] private float progressiveShutdownInterval = 1f;
    [SerializeField, Range(0f, 1f)] private float failureAmbientLightMultiplier = 0.5f;

    private Coroutine warningBlinkCoroutine;
    private Coroutine progressiveShutdownCoroutine;

    private void OnEnable()
    {
        if (taskManager == null)
        {
            return;
        }

        taskManager.TaskShown += HandleTaskShown;
        taskManager.TaskResolved += HandleTaskResolved;
        taskManager.EffectsTriggered += HandleEffects;
    }

    private void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.TaskShown -= HandleTaskShown;
            taskManager.TaskResolved -= HandleTaskResolved;
            taskManager.EffectsTriggered -= HandleEffects;
        }

        StopWarningBlink(true);
    }

    private void Start()
    {
        SetLightsEnabled(emergencyLights, false);
    }

    public string GetInteractionText()
    {
        return interactionText;
    }

    public bool CanInteract()
    {
        TaskData currentTask = taskManager != null ? taskManager.GetCurrentTask() : null;

        return currentTask != null
            && currentTask.taskId == emergencyTaskId
            && !taskManager.IsCurrentTaskResolved();
    }

    public void Interact()
    {
        if (!CanInteract())
        {
            Debug.LogWarning("La iluminacion de emergencia no puede activarse en este momento.");
            return;
        }

        Debug.Log("Iluminacion de emergencia solicitada. Enviando AUTORIZAR.");
        taskManager.ResolveCurrentTask(DecisionType.Authorize);
    }

    private void HandleTaskShown(TaskData task)
    {
        if (task == null || task.taskId != emergencyTaskId)
        {
            return;
        }

        StopWarningBlink(true);
        warningBlinkCoroutine = StartCoroutine(BlinkMainLights());
        Debug.Log("Baja tension detectada: las luces principales comenzaron a parpadear.");
    }

    private void HandleTaskResolved(TaskData task, bool wasCorrect)
    {
        if (task != null && task.taskId == emergencyTaskId)
        {
            StopWarningBlink(true);
        }
    }

    private void HandleEffects(List<TaskEffectType> effects)
    {
        if (effects == null)
        {
            return;
        }

        if (effects.Contains(TaskEffectType.ActivateEmergencyLighting))
        {
            ActivateEmergencyLighting();
        }

        if (effects.Contains(TaskEffectType.ProgressiveLightFailure))
        {
            StartProgressiveShutdown();
        }
    }

    private void ActivateEmergencyLighting()
    {
        StopWarningBlink(true);
        SetLightsEnabled(mainLights, false);
        SetLightsEnabled(emergencyLights, true);
        Debug.Log("Iluminacion de emergencia ACTIVADA. Consumo energetico reducido.");
    }

    private void StartProgressiveShutdown()
    {
        StopWarningBlink(true);

        RenderSettings.ambientIntensity *= failureAmbientLightMultiplier;
        Debug.Log("La iluminacion ambiental general bajo a: " + RenderSettings.ambientIntensity);

        if (progressiveShutdownCoroutine != null)
        {
            StopCoroutine(progressiveShutdownCoroutine);
        }

        progressiveShutdownCoroutine = StartCoroutine(ShutdownMainLightsProgressively());
    }

    private IEnumerator BlinkMainLights()
    {
        bool lightsEnabled = true;

        while (true)
        {
            lightsEnabled = !lightsEnabled;
            SetLightsEnabled(mainLights, lightsEnabled);
            yield return new WaitForSeconds(Mathf.Max(0.05f, warningBlinkInterval));
        }
    }

    private IEnumerator ShutdownMainLightsProgressively()
    {
        if (mainLights == null)
        {
            progressiveShutdownCoroutine = null;
            yield break;
        }

        foreach (Light mainLight in mainLights)
        {
            if (mainLight == null)
            {
                continue;
            }

            mainLight.enabled = false;
            Debug.Log("Una luz principal se apago por deterioro de energia: " + mainLight.name);
            yield return new WaitForSeconds(Mathf.Max(0.05f, progressiveShutdownInterval));
        }

        progressiveShutdownCoroutine = null;
    }

    private void StopWarningBlink(bool restoreMainLights)
    {
        if (warningBlinkCoroutine == null)
        {
            return;
        }

        StopCoroutine(warningBlinkCoroutine);
        warningBlinkCoroutine = null;

        if (restoreMainLights)
        {
            SetLightsEnabled(mainLights, true);
        }
    }

    private void SetLightsEnabled(Light[] lights, bool isEnabled)
    {
        if (lights == null)
        {
            return;
        }

        foreach (Light assignedLight in lights)
        {
            if (assignedLight != null)
            {
                assignedLight.enabled = isEnabled;
            }
        }
    }
}
