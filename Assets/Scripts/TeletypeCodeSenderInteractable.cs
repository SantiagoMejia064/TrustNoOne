using System.Collections;
using TMPro;
using UnityEngine;

public class TeletypeCodeSenderInteractable : MonoBehaviour, IInteractable
{
    [Header("Task")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private string taskId = "Tarea8";
    [SerializeField] private BotonAutorizar botonAutorizar;
    [SerializeField] private BotonRechazar botonRechazar;

    [Header("Interaction")]
    [SerializeField] private string interactionText = "E - Send code B-17";
    [SerializeField] private string interactableName = "Teletype code sender";

    [Header("Code")]
    [SerializeField] private string codeToSend = "B-17";
    [SerializeField] private TextMeshProUGUI teletypeOutputText;
    [SerializeField] private bool typeMessageLetterByLetter = true;
    [SerializeField] private float charactersPerSecond = 35f;
    [TextArea(2, 5)]
    [SerializeField] private string sentMessage = "> SENDING CODE B-17...\n> CODE ACCEPTED\n> REMOTE LOCKDOWN STARTED";

    [Header("Feedback")]
    [SerializeField] private GameObject[] activatedAfterSending;
    [SerializeField] private GameObject[] deactivatedAfterSending;
    [SerializeField] private AudioSource sendAudio;
    [SerializeField] private AudioSource alarmAudio;

    private bool codeSent;
    private Coroutine printRoutine;

    // Detiene cualquier escritura pendiente si el objeto se desactiva.
    private void OnDisable()
    {
        if (printRoutine != null)
        {
            StopCoroutine(printRoutine);
            printRoutine = null;
        }
    }

    // Muestra el texto de interaccion cuando el jugador mira el emisor.
    public string GetInteractionText()
    {
        return interactionText;
    }

    // Permite enviar el codigo solo durante la tarea indicada.
    public bool CanInteract()
    {
        return !codeSent
            && taskManager != null
            && taskManager.CurrentTask != null
            && taskManager.CurrentTask.Id == taskId;
    }

    // Envia el codigo, muestra feedback y habilita Autorizar.
    public void Interact()
    {
        if (!CanInteract())
        {
            return;
        }

        Debug.Log("Interactuando con: " + interactableName + " | Codigo enviado: " + codeToSend);

        codeSent = true;
        PlaySendFeedback();
        PrintSentMessage();

        botonAutorizar?.Activar();
        botonRechazar?.Desactivar();
    }

    // Ejecuta los cambios visuales y de audio al enviar el codigo.
    private void PlaySendFeedback()
    {
        SetObjectsActive(activatedAfterSending, true);
        SetObjectsActive(deactivatedAfterSending, false);

        if (sendAudio != null)
        {
            sendAudio.Play();
        }

        if (alarmAudio != null)
        {
            alarmAudio.Play();
        }
    }

    // Escribe el resultado del envio en el texto del teletipo.
    private void PrintSentMessage()
    {
        if (teletypeOutputText == null)
        {
            return;
        }

        if (printRoutine != null)
        {
            StopCoroutine(printRoutine);
        }

        if (typeMessageLetterByLetter)
        {
            printRoutine = StartCoroutine(TypeText(sentMessage));
            return;
        }

        teletypeOutputText.text = sentMessage;
    }

    // Simula que el teletipo imprime el mensaje caracter por caracter.
    private IEnumerator TypeText(string text)
    {
        teletypeOutputText.text = string.Empty;

        float delay = charactersPerSecond <= 0f ? 0f : 1f / charactersPerSecond;

        for (int i = 0; i < text.Length; i++)
        {
            teletypeOutputText.text += text[i];

            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }
        }

        printRoutine = null;
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
}