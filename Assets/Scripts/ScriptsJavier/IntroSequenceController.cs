using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;


public class IntroSequenceController : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject introPanel;
    [SerializeField] private TextMeshProUGUI introText;
    [SerializeField] private GameObject continuePrompt;

    [Header("Audio de la radio")]
    [SerializeField] private AudioSource radioAudio;

    [Header("Lineas del dialogo (en orden, en ingles)")]
    [SerializeField, TextArea(2, 4)]
    private List<string> lines = new List<string>
    {
        "This is Command. Do you copy?",
        "You are the night operator of this facility.",
        "Your job: review every incoming order from this terminal.",
        "Some orders will be legitimate. Others will be sabotage attempts.",
        "Authorize what keeps this station running. Reject what threatens it.",
        "Approve four acts of sabotage, or reject four legitimate orders, and this facility falls.",
        "Complete all eight tasks correctly and you win.",
        "Look around with your mouse. Approach a task and press E to interact.",
        "Stay sharp. Command out."
    };

    [Header("Configuracion de escritura")]
    [SerializeField] private float charactersPerSecond = 30f;

    [Header("Referencias a bloquear durante la intro")]
    [SerializeField] private PlayerInteractor playerInteractor;
    [SerializeField] private Movimiento movimiento;
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private LinternaController linternaController;

    private int currentLineIndex = -1;
    private bool isTyping;
    private bool sequenceFinished;
    private Coroutine typingRoutine;

    private void Start()
    {
        BloquearControlesDeJuego();

        if (introPanel != null)
        {
            introPanel.SetActive(true);
        }

        if (radioAudio != null)
        {
            radioAudio.Play();
        }

        AvanzarLinea();
    }

    private void Update()
    {
        if (sequenceFinished)
        {
            return;
        }

        if (WasAdvancePressed())
        {
            if (isTyping)
            {
                CompletarLineaInstantaneamente();
            }
            else
            {
                AvanzarLinea();
            }
        }
    }

    private bool WasAdvancePressed()
    {
        bool teclaPresionada = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        bool clicPresionado = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        return teclaPresionada || clicPresionado;
    }

    private void AvanzarLinea()
    {
        currentLineIndex++;

        if (currentLineIndex >= lines.Count)
        {
            TerminarIntro();
            return;
        }

        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
        }

        typingRoutine = StartCoroutine(EscribirLinea(lines[currentLineIndex]));
    }

    private IEnumerator EscribirLinea(string linea)
    {
        isTyping = true;

        if (continuePrompt != null)
        {
            continuePrompt.SetActive(false);
        }

        introText.text = string.Empty;
        float delayPorCaracter = 1f / Mathf.Max(1f, charactersPerSecond);

        foreach (char caracter in linea)
        {
            introText.text += caracter;
            yield return new WaitForSeconds(delayPorCaracter);
        }

        isTyping = false;

        if (continuePrompt != null)
        {
            continuePrompt.SetActive(true);
        }
    }

    private void CompletarLineaInstantaneamente()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        introText.text = lines[currentLineIndex];
        isTyping = false;

        if (continuePrompt != null)
        {
            continuePrompt.SetActive(true);
        }
    }

    private void TerminarIntro()
    {
        sequenceFinished = true;

        if (introPanel != null)
        {
            introPanel.SetActive(false);
        }

        DesbloquearControlesDeJuego();

        if (taskManager != null)
        {
            taskManager.StartRun();
        }
    }

    private void BloquearControlesDeJuego()
    {
        if (playerInteractor != null)
        {
            playerInteractor.enabled = false;
        }

        if (movimiento != null)
        {
            movimiento.SetCameraZonesEnabled(false);
        }

        if (linternaController != null)
        {
            linternaController.enabled = false;
            linternaController.OcultarTexto();
        }
    }

    private void DesbloquearControlesDeJuego()
    {
        if (playerInteractor != null)
        {
            playerInteractor.enabled = true;
        }

        if (movimiento != null)
        {
            movimiento.SetCameraZonesEnabled(true);
        }

        if (linternaController != null)
        {
            linternaController.enabled = true;
            linternaController.MostrarTexto();
        }
    }
}