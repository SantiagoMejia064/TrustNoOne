using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CommunicationsRestartController : MonoBehaviour, IInteractable, IHoldInteractable
{
    [Header("Tarea")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private string restartTaskId = "reiniciar_comunicaciones";
    [SerializeField] private string interactionText = "MANTENER E - REINICIAR COMUNICACIONES";
    [SerializeField] private float holdDuration = 3f;

    [Header("Bloqueo temporal")]
    [SerializeField] private float disabledDuration = 90f;
    [SerializeField] private Behaviour[] radioControls;
    [SerializeField] private Behaviour[] phoneControls;
    [SerializeField] private Collider[] radioInteractionColliders;
    [SerializeField] private Collider[] phoneInteractionColliders;

    [Header("Mensajes de estado")]
    [SerializeField] private TMP_Text radioStatusText;
    [SerializeField] private TMP_Text phoneStatusText;
    [SerializeField] private string disabledMessage = "SISTEMA DESACTIVADO";

    private bool[] radioPreviousStates;
    private bool[] phonePreviousStates;
    private bool[] radioColliderPreviousStates;
    private bool[] phoneColliderPreviousStates;
    private string previousRadioText;
    private string previousPhoneText;
    private Coroutine restoreCoroutine;

    private void OnEnable()
    {
        if (taskManager != null)
        {
            taskManager.EffectsTriggered += HandleEffects;
        }
    }

    private void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.EffectsTriggered -= HandleEffects;
        }

        if (restoreCoroutine != null)
        {
            StopCoroutine(restoreCoroutine);
            restoreCoroutine = null;
            RestoreCommunications();
        }
    }

    public string GetInteractionText()
    {
        return interactionText;
    }

    public bool CanInteract()
    {
        TaskData currentTask = taskManager != null ? taskManager.GetCurrentTask() : null;

        return currentTask != null
            && currentTask.taskId == restartTaskId
            && !taskManager.IsCurrentTaskResolved();
    }

    public float GetHoldDuration()
    {
        return holdDuration;
    }

    public void Interact()
    {
        if (!CanInteract())
        {
            Debug.LogWarning("El sistema de comunicaciones no puede reiniciarse en este momento.");
            return;
        }

        Debug.Log("Reinicio de comunicaciones solicitado. Enviando AUTORIZAR.");
        taskManager.ResolveCurrentTask(DecisionType.Authorize);
    }

    private void HandleEffects(List<TaskEffectType> effects)
    {
        if (effects == null || !effects.Contains(TaskEffectType.DisableCommunicationsTemporarily))
        {
            return;
        }

        DisableCommunications();
    }

    private void DisableCommunications()
    {
        if (restoreCoroutine != null)
        {
            StopCoroutine(restoreCoroutine);
            RestoreCommunications();
        }

        previousRadioText = radioStatusText != null ? radioStatusText.text : string.Empty;
        previousPhoneText = phoneStatusText != null ? phoneStatusText.text : string.Empty;

        radioPreviousStates = DisableControls(radioControls);
        phonePreviousStates = DisableControls(phoneControls);
        radioColliderPreviousStates = DisableColliders(radioInteractionColliders);
        phoneColliderPreviousStates = DisableColliders(phoneInteractionColliders);
        SetStatusText(radioStatusText, disabledMessage);
        SetStatusText(phoneStatusText, disabledMessage);

        Debug.Log("Sabotaje: radio y telefono fuera de servicio durante " + disabledDuration + " segundos.");
        restoreCoroutine = StartCoroutine(RestoreAfterDelay());
    }

    private IEnumerator RestoreAfterDelay()
    {
        yield return new WaitForSeconds(Mathf.Max(0.1f, disabledDuration));
        restoreCoroutine = null;
        RestoreCommunications();
    }

    private void RestoreCommunications()
    {
        RestoreControls(radioControls, radioPreviousStates);
        RestoreControls(phoneControls, phonePreviousStates);
        RestoreColliders(radioInteractionColliders, radioColliderPreviousStates);
        RestoreColliders(phoneInteractionColliders, phoneColliderPreviousStates);
        SetStatusText(radioStatusText, previousRadioText);
        SetStatusText(phoneStatusText, previousPhoneText);
        Debug.Log("Radio y telefono nuevamente disponibles.");
    }

    private bool[] DisableControls(Behaviour[] controls)
    {
        if (controls == null)
        {
            return new bool[0];
        }

        bool[] previousStates = new bool[controls.Length];

        for (int i = 0; i < controls.Length; i++)
        {
            Behaviour control = controls[i];

            if (control == null || control == this)
            {
                continue;
            }

            previousStates[i] = control.enabled;
            control.enabled = false;
        }

        return previousStates;
    }

    private void RestoreControls(Behaviour[] controls, bool[] previousStates)
    {
        if (controls == null || previousStates == null)
        {
            return;
        }

        int count = Mathf.Min(controls.Length, previousStates.Length);

        for (int i = 0; i < count; i++)
        {
            if (controls[i] != null && controls[i] != this)
            {
                controls[i].enabled = previousStates[i];
            }
        }
    }

    private bool[] DisableColliders(Collider[] colliders)
    {
        if (colliders == null)
        {
            return new bool[0];
        }

        bool[] previousStates = new bool[colliders.Length];

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] == null)
            {
                continue;
            }

            previousStates[i] = colliders[i].enabled;
            colliders[i].enabled = false;
        }

        return previousStates;
    }

    private void RestoreColliders(Collider[] colliders, bool[] previousStates)
    {
        if (colliders == null || previousStates == null)
        {
            return;
        }

        int count = Mathf.Min(colliders.Length, previousStates.Length);

        for (int i = 0; i < count; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].enabled = previousStates[i];
            }
        }
    }

    private void SetStatusText(TMP_Text targetText, string value)
    {
        if (targetText != null)
        {
            targetText.text = value;
        }
    }
}
