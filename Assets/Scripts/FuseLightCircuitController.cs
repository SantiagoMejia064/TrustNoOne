using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuseLightCircuitController : MonoBehaviour
{
    [Header("Task")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private string fuseTaskId = "sustituir_fusible_luces";

    [Header("Lights")]
    [SerializeField] private Light[] affectedLights;
    [SerializeField] private bool turnLightsOffWhenFailed = true;
    [SerializeField, Range(0f, 1f)] private float failedIntensityMultiplier = 0.15f;

    [Header("Indicators")]
    [SerializeField] private Renderer[] indicatorRenderers;
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color failedColor = Color.red;

    [Header("Flicker")]
    [SerializeField] private bool flickerWhenTaskIsActive = true;
    [SerializeField] private float flickerInterval = 0.12f;
    [SerializeField, Range(0f, 1f)] private float flickerMinIntensityMultiplier = 0.25f;

    private readonly Dictionary<Light, float> originalIntensities = new Dictionary<Light, float>();
    private readonly Dictionary<Light, bool> originalEnabledStates = new Dictionary<Light, bool>();
    private Coroutine flickerRoutine;

    // Guarda el estado original de las luces para poder restaurarlas despues.
    private void Awake()
    {
        CacheOriginalLightStates();
    }

    // Escucha tareas y efectos para cambiar el circuito de luces cuando corresponda.
    private void OnEnable()
    {
        if (taskManager != null)
        {
            taskManager.TaskShown += HandleTaskShown;
            taskManager.TaskResolved += HandleTaskResolved;
            taskManager.EffectsTriggered += HandleEffects;
        }
    }

    // Deja de escuchar eventos y detiene el parpadeo al desactivarse.
    private void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.TaskShown -= HandleTaskShown;
            taskManager.TaskResolved -= HandleTaskResolved;
            taskManager.EffectsTriggered -= HandleEffects;
        }

        StopFlicker();
    }

    // Marca el circuito como reparado y restaura las luces normales.
    public void SetCircuitHealthy()
    {
        StopFlicker();
        RestoreOriginalLights();
        SetIndicatorColor(healthyColor);
    }

    // Marca el circuito como fallado y apaga o baja la intensidad de las luces.
    public void SetCircuitFailed()
    {
        StopFlicker();

        foreach (Light affectedLight in affectedLights)
        {
            if (affectedLight == null)
            {
                continue;
            }

            affectedLight.enabled = !turnLightsOffWhenFailed;

            if (originalIntensities.TryGetValue(affectedLight, out float originalIntensity))
            {
                affectedLight.intensity = originalIntensity * failedIntensityMultiplier;
            }
        }

        SetIndicatorColor(failedColor);
    }

    // Activa el estado de advertencia cuando aparece la tarea del fusible.
    private void HandleTaskShown(TaskData task)
    {
        if (task == null || task.taskId != fuseTaskId)
        {
            return;
        }

        SetIndicatorColor(warningColor);

        if (flickerWhenTaskIsActive)
        {
            StartFlicker();
        }
    }

    // Decide si reparar o fallar el circuito segun el resultado de la tarea.
    private void HandleTaskResolved(TaskData task, bool wasCorrect)
    {
        if (task == null || task.taskId != fuseTaskId)
        {
            return;
        }

        if (wasCorrect)
        {
            SetCircuitHealthy();
        }
        else
        {
            SetCircuitFailed();
        }
    }

    // Reacciona a efectos secundarios como BurnLights aunque vengan de otra tarea.
    private void HandleEffects(List<TaskEffectType> effects)
    {
        if (effects == null)
        {
            return;
        }

        foreach (TaskEffectType effect in effects)
        {
            if (effect == TaskEffectType.BurnLights)
            {
                SetCircuitFailed();
            }
        }
    }

    // Comienza el parpadeo para indicar que el circuito tiene problema.
    private void StartFlicker()
    {
        StopFlicker();
        flickerRoutine = StartCoroutine(FlickerLights());
    }

    // Detiene el parpadeo activo si existe.
    private void StopFlicker()
    {
        if (flickerRoutine != null)
        {
            StopCoroutine(flickerRoutine);
            flickerRoutine = null;
        }
    }

    // Cambia la intensidad de las luces repetidamente para simular falla electrica.
    private IEnumerator FlickerLights()
    {
        while (true)
        {
            foreach (Light affectedLight in affectedLights)
            {
                if (affectedLight == null || !originalIntensities.TryGetValue(affectedLight, out float originalIntensity))
                {
                    continue;
                }

                affectedLight.enabled = true;
                affectedLight.intensity = originalIntensity * Random.Range(flickerMinIntensityMultiplier, 1f);
            }

            yield return new WaitForSeconds(Mathf.Max(0.01f, flickerInterval));
        }
    }

    // Guarda intensidad y encendido original de cada luz asignada.
    private void CacheOriginalLightStates()
    {
        originalIntensities.Clear();
        originalEnabledStates.Clear();

        foreach (Light affectedLight in affectedLights)
        {
            if (affectedLight == null)
            {
                continue;
            }

            originalIntensities[affectedLight] = affectedLight.intensity;
            originalEnabledStates[affectedLight] = affectedLight.enabled;
        }
    }

    // Devuelve cada luz al estado que tenia al empezar la escena.
    private void RestoreOriginalLights()
    {
        foreach (Light affectedLight in affectedLights)
        {
            if (affectedLight == null)
            {
                continue;
            }

            if (originalEnabledStates.TryGetValue(affectedLight, out bool originalEnabled))
            {
                affectedLight.enabled = originalEnabled;
            }

            if (originalIntensities.TryGetValue(affectedLight, out float originalIntensity))
            {
                affectedLight.intensity = originalIntensity;
            }
        }
    }

    // Cambia el color de indicadores del panel para comunicar el estado del circuito.
    private void SetIndicatorColor(Color color)
    {
        foreach (Renderer indicatorRenderer in indicatorRenderers)
        {
            if (indicatorRenderer == null)
            {
                continue;
            }

            indicatorRenderer.material.color = color;
        }
    }
}
