using TMPro;
using UnityEngine;

public class TeletypeController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI verifyText;

    [Header("Power")]
    [SerializeField] private bool isPowered = true;
    [SerializeField] private GameObject poweredVisuals;
    [SerializeField] private GameObject unpoweredVisuals;
    [SerializeField] private TextMeshProUGUI powerStateText;
    [SerializeField] private string poweredStateMessage = "TELETIPO OPERATIVO";
    [SerializeField] private string unpoweredStateMessage = "TELETIPO SIN CORRIENTE";
    [SerializeField] private string rebootingStateMessage = "TELETIPO REINICIANDO";

    public bool IsPowered => isPowered;

    // Aplica el estado visual inicial del teletipo al empezar la escena.
    private void Start()
    {
        ApplyPowerVisuals();
    }

    // Muestra la informacion de la tarea si el teletipo tiene corriente.
    public void ShowTask(TaskData task)
    {
        if (!isPowered)
        {
            Clear();
            return;
        }

        if (task == null)
        {
            Debug.LogWarning("No se puede mostrar una tarea nula en el teletipo.");
            Clear();
            return;
        }

        if (titleText != null)
        {
            titleText.text = task.title;
        }
        else
        {
            Debug.LogWarning("Falta asignar titleText en TeletypeController.");
        }

        if (messageText != null)
        {
            messageText.text = task.message;
        }
        else
        {
            Debug.LogWarning("Falta asignar messageText en TeletypeController.");
        }

        if (verifyText != null)
        {
            verifyText.text = task.howToVerify;
        }
        else
        {
            Debug.LogWarning("Falta asignar verifyText en TeletypeController.");
        }
    }

    // Limpia los textos para que el teletipo no muestre informacion vieja.
    public void Clear()
    {
        if (titleText != null)
        {
            titleText.text = string.Empty;
        }

        if (messageText != null)
        {
            messageText.text = string.Empty;
        }

        if (verifyText != null)
        {
            verifyText.text = string.Empty;
        }
    }

    // Cambia el estado de corriente y actualiza textos/paneles asociados.
    public void SetPowered(bool powered)
    {
        if (isPowered == powered)
        {
            ApplyPowerVisuals();
            return;
        }

        isPowered = powered;

        if (!isPowered)
        {
            Clear();
        }

        ApplyPowerVisuals();
    }

    // Enciende el teletipo desde otros scripts o eventos.
    public void TurnOn()
    {
        SetPowered(true);
    }

    // Apaga el teletipo desde otros scripts o eventos.
    public void TurnOff()
    {
        SetPowered(false);
    }

    // Muestra el estado de reinicio mientras el teletipo aun no puede usarse.
    public void ShowRebootingState()
    {
        Clear();
        SetPowered(false);
        SetPowerStateMessage(rebootingStateMessage);
    }

    // Permite escribir un mensaje de estado especifico en el panel de energia.
    public void SetPowerStateMessage(string message)
    {
        if (powerStateText != null)
        {
            powerStateText.text = message;
        }
    }

    // Activa los objetos visuales correctos segun si el teletipo esta encendido o apagado.
    private void ApplyPowerVisuals()
    {
        if (poweredVisuals != null)
        {
            poweredVisuals.SetActive(isPowered);
        }

        if (unpoweredVisuals != null)
        {
            unpoweredVisuals.SetActive(!isPowered);
        }

        if (powerStateText != null)
        {
            powerStateText.text = isPowered ? poweredStateMessage : unpoweredStateMessage;
        }
    }
}
