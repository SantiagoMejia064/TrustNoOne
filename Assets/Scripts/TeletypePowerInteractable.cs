using UnityEngine;

public class TeletypePowerInteractable : MonoBehaviour, IInteractable
{
    [Header("Task")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private string taskId = "Tarea8";
    [SerializeField] private BotonAutorizar botonAutorizar;
    [SerializeField] private BotonRechazar botonRechazar;

    [Header("Interaction")]
    [SerializeField] private string interactionText = "E - Disconnect teletype";
    [SerializeField] private string interactableName = "Teletype power";

    [Header("Power")]
    [SerializeField] private bool startsPowered = true;
    [SerializeField] private GameObject[] poweredVisuals;
    [SerializeField] private GameObject[] unpoweredVisuals;
    [SerializeField] private Behaviour[] disableWhenUnpowered;
    [SerializeField] private AudioSource poweredAudio;

    private bool isPowered;
    private bool taskDone;

    // Coloca el teletipo en su estado inicial.
    private void Awake()
    {
        SetPowered(startsPowered);
    }

    // Entrega el texto que aparece al mirar el interruptor del teletipo.
    public string GetInteractionText()
    {
        return interactionText;
    }

    // Permite usar el interruptor solo durante la Tarea8.
    public bool CanInteract()
    {
        return !taskDone
            && taskManager != null
            && taskManager.CurrentTask != null
            && taskManager.CurrentTask.Id == taskId;
    }

    // Apaga el teletipo y habilita el boton Autorizar.
    public void Interact()
    {
        if (!CanInteract())
        {
            return;
        }

        Debug.Log("Interactuando con: " + interactableName);

        taskDone = true;
        SetPowered(false);

        botonAutorizar?.Activar();
        botonRechazar?.Desactivar();
    }

    // Cambia el estado visual y funcional del teletipo.
    private void SetPowered(bool value)
    {
        isPowered = value;

        SetObjectsActive(poweredVisuals, isPowered);
        SetObjectsActive(unpoweredVisuals, !isPowered);
        SetBehavioursEnabled(disableWhenUnpowered, isPowered);

        if (poweredAudio == null)
        {
            return;
        }

        if (isPowered)
        {
            poweredAudio.Play();
        }
        else
        {
            poweredAudio.Stop();
        }
    }

    // Activa o desactiva una lista de objetos.
    private void SetObjectsActive(GameObject[] objects, bool value)
    {
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
            {
                objects[i].SetActive(value);
            }
        }
    }

    // Activa o desactiva componentes que dependen de la corriente.
    private void SetBehavioursEnabled(Behaviour[] behaviours, bool value)
    {
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] != null)
            {
                behaviours[i].enabled = value;
            }
        }
    }
}