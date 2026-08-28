using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AlarmaO2PanelSwitchJLV : MonoBehaviour, IInteractable
{
    [Header("Referencias")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private AudioSource audioAlarma;

    [Header("Texto")]
    [SerializeField] private string interactionText = "E - Apagar alarma";

    [Header("Audio")]
    [SerializeField] private float duracionFadeOut = 0.6f;

    [Header("Estado")]
    [SerializeField] private bool desbloqueado = false;
    [SerializeField] private bool alarmaApagada = false;

    private float volumenOriginal;

    private void Awake()
    {
        if (audioAlarma != null)
        {
            volumenOriginal = audioAlarma.volume;
        }
    }

    private void OnEnable()
    {
        if (taskManager != null)
        {
            taskManager.EffectsTriggered += HandleEffectsTriggered;
        }
    }

    private void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.EffectsTriggered -= HandleEffectsTriggered;
        }
    }

    private void HandleEffectsTriggered(List<TaskEffectType> effects)
    {
        if (effects == null || !effects.Contains(TaskEffectType.StopAlarm))
        {
            return;
        }

        desbloqueado = true;
        Debug.Log("[Caso 8] Alarma desbloqueada en el panel. Ya se puede apagar fisicamente.");
    }

    public string GetInteractionText()
    {
        return interactionText;
    }

    public bool CanInteract()
    {
        return desbloqueado && !alarmaApagada;
    }

    public void Interact()
    {
        if (!CanInteract())
        {
            return;
        }

        alarmaApagada = true;
        StartCoroutine(FadeOutYDetener());
    }

    private IEnumerator FadeOutYDetener()
    {
        if (audioAlarma != null)
        {
            float t = 0f;
            while (t < duracionFadeOut)
            {
                t += Time.deltaTime;
                audioAlarma.volume = Mathf.Lerp(volumenOriginal, 0f, t / duracionFadeOut);
                yield return null;
            }

            audioAlarma.Stop();
            audioAlarma.volume = volumenOriginal;
        }

        Debug.Log("[Caso 8] Alarma apagada fisicamente desde el panel.");

     
        if (taskManager != null)
        {
            taskManager.ShowNextTask();
        }
    }
}