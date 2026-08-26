using System.Collections.Generic;
using UnityEngine;

public class TeletypeTaskEffectHandler : MonoBehaviour
{
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private TeletypeController teletypeController;
    [SerializeField] private TeletypePowerSwitch teletypePowerSwitch;

    // Se suscribe a los efectos de tareas para reaccionar cuando una tarea termina.
    private void OnEnable()
    {
        if (taskManager != null)
        {
            taskManager.EffectsTriggered += HandleEffects;
        }
    }

    // Se desuscribe para evitar llamadas a este objeto cuando ya no este activo.
    private void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.EffectsTriggered -= HandleEffects;
        }
    }

    // Revisa los efectos recibidos y aplica solo los que afectan al teletipo.
    private void HandleEffects(List<TaskEffectType> effects)
    {
        if (effects == null)
        {
            return;
        }

        foreach (TaskEffectType effect in effects)
        {
            if (effect == TaskEffectType.DisableTeletype)
            {
                SetTeletypePowered(false);
            }
            else if (effect == TaskEffectType.EnableTeletype)
            {
                SetTeletypePowered(true);
            }
        }
    }

    // Sincroniza el controlador visual y el switch con el mismo estado de energia.
    private void SetTeletypePowered(bool powered)
    {
        if (teletypeController != null)
        {
            teletypeController.SetPowered(powered);
        }

        if (teletypePowerSwitch != null)
        {
            teletypePowerSwitch.SetPowered(powered);
        }
    }
}
