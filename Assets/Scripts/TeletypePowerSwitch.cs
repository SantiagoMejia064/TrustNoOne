using System.Collections;
using UnityEngine;

public class TeletypePowerSwitch : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private TeletypeController teletypeController;

    [Header("Task")]
    [SerializeField] private string teletypePowerTaskId = "cortar_corriente_teletipo";
    [SerializeField] private bool resolveTaskWhenTurnedOff = true;

    [Header("Power")]
    [SerializeField] private bool isPowered = true;
    [SerializeField] private bool allowRestore = true;
    [SerializeField] private float rebootDuration = 6f;

    [Header("Interaction")]
    [SerializeField] private string turnOffText = "E - CORTAR CORRIENTE";
    [SerializeField] private string turnOnText = "E - REINICIAR TELETIPO";
    [SerializeField] private string rebootingText = "REINICIANDO...";
    [SerializeField] private string lockedOffText = "SIN CORRIENTE";

    [Header("Visuals")]
    [SerializeField] private Transform switchHandle;
    [SerializeField] private Vector3 poweredLocalEulerAngles;
    [SerializeField] private Vector3 unpoweredLocalEulerAngles = new Vector3(-45f, 0f, 0f);
    [SerializeField] private Vector3 rebootingLocalEulerAngles = new Vector3(-20f, 0f, 0f);
    [SerializeField] private GameObject poweredVisuals;
    [SerializeField] private GameObject unpoweredVisuals;
    [SerializeField] private GameObject rebootingVisuals;

    private bool isRebooting;
    private bool taskResolvedBySwitch;
    private Coroutine rebootRoutine;

    // Se suscribe al cambio de tarea para poder reutilizar el switch en otra ronda.
    private void OnEnable()
    {
        if (taskManager != null)
        {
            taskManager.TaskShown += HandleTaskShown;
        }
    }

    // Cancela la suscripcion cuando el objeto deja de estar activo.
    private void OnDisable()
    {
        if (taskManager != null)
        {
            taskManager.TaskShown -= HandleTaskShown;
        }
    }

    // Aplica el estado inicial del switch y del teletipo al arrancar.
    private void Start()
    {
        ApplyPowerState();
    }

    // Devuelve el texto que vera el jugador segun el estado actual.
    public string GetInteractionText()
    {
        if (isRebooting)
        {
            return rebootingText;
        }

        if (!isPowered && !allowRestore)
        {
            return lockedOffText;
        }

        return isPowered ? turnOffText : turnOnText;
    }

    // Permite interactuar solo si no esta reiniciando y el estado lo permite.
    public bool CanInteract()
    {
        return !isRebooting && (isPowered || allowRestore);
    }

    // Cambia entre apagar el teletipo o iniciar su reinicio.
    public void Interact()
    {
        if (isRebooting)
        {
            return;
        }

        if (isPowered)
        {
            SetPowered(false);
            TryResolveTeletypePowerTask();
            return;
        }

        if (allowRestore)
        {
            StartReboot();
        }
    }

    // Fuerza el estado de energia desde otros scripts o efectos.
    public void SetPowered(bool powered)
    {
        StopReboot();
        isPowered = powered;
        ApplyPowerState();
    }

    // Reinicia el bloqueo de resolucion cuando aparece esta tarea otra vez.
    private void HandleTaskShown(TaskData task)
    {
        if (task != null && task.taskId == teletypePowerTaskId)
        {
            taskResolvedBySwitch = false;
        }
    }

    // Comienza el tiempo muerto antes de volver a encender el teletipo.
    private void StartReboot()
    {
        StopReboot();
        isRebooting = true;
        isPowered = false;
        ApplyPowerState();
        rebootRoutine = StartCoroutine(CompleteRebootAfterDelay());
    }

    // Espera el tiempo configurado y luego deja el teletipo operativo.
    private IEnumerator CompleteRebootAfterDelay()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, rebootDuration));
        rebootRoutine = null;
        isRebooting = false;
        isPowered = true;
        ApplyPowerState();
    }

    // Detiene cualquier reinicio activo para evitar estados mezclados.
    private void StopReboot()
    {
        if (rebootRoutine != null)
        {
            StopCoroutine(rebootRoutine);
            rebootRoutine = null;
        }

        isRebooting = false;
    }

    // Sincroniza el teletipo, la palanca y los visuales con el estado actual.
    private void ApplyPowerState()
    {
        if (teletypeController != null)
        {
            if (isRebooting)
            {
                teletypeController.ShowRebootingState();
            }
            else
            {
                teletypeController.SetPowered(isPowered);
            }
        }

        if (switchHandle != null)
        {
            Vector3 targetEulerAngles = GetCurrentHandleEulerAngles();
            switchHandle.localRotation = Quaternion.Euler(targetEulerAngles);
        }

        if (poweredVisuals != null)
        {
            poweredVisuals.SetActive(isPowered && !isRebooting);
        }

        if (unpoweredVisuals != null)
        {
            unpoweredVisuals.SetActive(!isPowered && !isRebooting);
        }

        if (rebootingVisuals != null)
        {
            rebootingVisuals.SetActive(isRebooting);
        }
    }

    // Decide la rotacion visual de la palanca segun el estado del switch.
    private Vector3 GetCurrentHandleEulerAngles()
    {
        if (isRebooting)
        {
            return rebootingLocalEulerAngles;
        }

        return isPowered ? poweredLocalEulerAngles : unpoweredLocalEulerAngles;
    }

    // Si la tarea activa pide cortar corriente, apagar el teletipo cuenta como autorizar.
    private void TryResolveTeletypePowerTask()
    {
        if (!resolveTaskWhenTurnedOff || taskResolvedBySwitch || taskManager == null)
        {
            return;
        }

        TaskData currentTask = taskManager.GetCurrentTask();

        if (currentTask == null || currentTask.taskId != teletypePowerTaskId)
        {
            return;
        }

        taskResolvedBySwitch = true;
        taskManager.ResolveCurrentTask(DecisionType.Authorize);
    }
}
