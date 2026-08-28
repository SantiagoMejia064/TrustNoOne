using System.Collections.Generic;
using UnityEngine;

public class TaskEffectsDebugLogger : MonoBehaviour
{
    [SerializeField] private TaskManager taskManager;

    private void OnEnable()
    {
        if (taskManager != null)
        {
            taskManager.EffectsTriggered += LogEffects;
        }
    }

    private void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.EffectsTriggered -= LogEffects;
        }
    }

    private void LogEffects(List<TaskEffectType> effects)
    {
        if (effects == null || effects.Count == 0)
        {
            return;
        }

        foreach (TaskEffectType effect in effects)
        {
            if (effect == TaskEffectType.None)
            {
                continue;
            }

            if (effect == TaskEffectType.DisableRandomInformationDevice)
            {
                string[] devices = { "RADIO", "TELEFONO", "TELETIPO" };
                string selectedDevice = devices[Random.Range(0, devices.Length)];
                Debug.Log("Fuente de informacion deshabilitada temporalmente: " + selectedDevice);
                continue;
            }

            if (effect == TaskEffectType.TriggerOxygenDistress)
            {
                Debug.Log("Falta de oxigeno: respiracion fuerte, vineta y perdida parcial de claridad visual.");
                continue;
            }

            Debug.Log("Efecto de tarea activado: " + effect);
        }
    }
}
