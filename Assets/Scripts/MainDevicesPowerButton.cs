using System.Collections.Generic;
using UnityEngine;

public class MainDevicesPowerButton : MonoBehaviour, IInteractable
{
    private static readonly List<MainDevicesPowerButton> activeButtons = new List<MainDevicesPowerButton>();

    [SerializeField] private string deviceName = "Dispositivo";
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private string overloadTaskId = "sobrecarga_electrica_entrante";
    [SerializeField] private bool isConnected = true;
    [SerializeField] private string disconnectText = "E - DESCONECTAR";
    [SerializeField] private string connectText = "E - CONECTAR";

    private void OnEnable()
    {
        if (!activeButtons.Contains(this))
        {
            activeButtons.Add(this);
        }
    }

    private void OnDisable()
    {
        activeButtons.Remove(this);
    }

    public string GetInteractionText()
    {
        return isConnected ? disconnectText : connectText;
    }

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        isConnected = !isConnected;
        Debug.Log(GetDeviceName() + ": " + (isConnected ? "CONECTADO" : "DESCONECTADO"));

        if (AreAllRequiredDevicesDisconnected())
        {
            TryResolveOverloadTask();
        }
    }

    private bool AreAllRequiredDevicesDisconnected()
    {
        if (activeButtons.Count == 0)
        {
            return false;
        }

        foreach (MainDevicesPowerButton button in activeButtons)
        {
            if (button != null && button.isConnected)
            {
                return false;
            }
        }

        return true;
    }

    private string GetDeviceName()
    {
        return string.IsNullOrWhiteSpace(deviceName) ? gameObject.name : deviceName;
    }

    private void TryResolveOverloadTask()
    {
        TaskData currentTask = taskManager != null ? taskManager.GetCurrentTask() : null;

        if (currentTask == null || currentTask.taskId != overloadTaskId)
        {
            Debug.Log("Todos los dispositivos estan desconectados, pero no hay una tarea de sobrecarga activa.");
            return;
        }

        Debug.Log("Todos los dispositivos principales fueron desconectados. Enviando AUTORIZAR.");
        taskManager.ResolveCurrentTask(DecisionType.Authorize);
    }
}
