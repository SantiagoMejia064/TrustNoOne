using System.Collections;
using UnityEngine;

public class BotonRechazar : MonoBehaviour, IInteractable
{
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private string activeInteractionText = "E - Reject";
    [SerializeField] private string inactiveInteractionText = "You did your homework, so you can't back out now";
    [SerializeField] private string interactionText = "E - Reject";
    [SerializeField] private string interactableName = "Rejected button";
    [SerializeField] private bool canInteract = true;
    [SerializeField] private Animator animatorPapel;
    [SerializeField] private string nombreAnimacionPapel = "PapelAnim";
    [SerializeField] private AudioSource audioPapel;
    [SerializeField] private Transform visualBoton;
    [SerializeField] private float escalaPulsado = 0.85f;
    [SerializeField] private float duracionPulsado = 0.15f;

    public FiltroAuxiliar filtroAuxiliar;
    public LucesEmergencia lucesEmergencia;

    private Vector3 escalaOriginal;

    private void Awake()
    {
        if (visualBoton != null)
        {
            escalaOriginal = visualBoton.localScale;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Activar();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public string GetInteractionText()
    {
        return interactionText;
    }

    public bool CanInteract()
    {
        return canInteract;
    }

    public void Interact()
    {
        Debug.Log("Interactuando con: " + interactableName);

        if (animatorPapel != null)
        {
            animatorPapel.Play(nombreAnimacionPapel, 0, 0f);
        }
        else if (visualBoton != null)
        {
            StartCoroutine(AnimarPulsado());
        }

        if (audioPapel != null)
        {
            audioPapel.Play();
        }

        if (taskManager == null)
        {
            Debug.LogWarning("BotonRechazar: taskManager no esta asignado.");
            return;
        }

        if (taskManager.CurrentTask == null)
        {
            Debug.LogWarning("BotonRechazar: no hay tarea actual.");
            return;
        }

        if (taskManager.CurrentTask.Id == "Tarea1")
        {
            
        }

        if (taskManager.CurrentTask.Id == "Tarea2")
        {
            lucesEmergencia.consecuencias();
        }

        if (taskManager.CurrentTask.Id == "Tarea3")
        {

        }

        if (taskManager.CurrentTask.Id == "Tarea4")
        {

        }

        if (taskManager.CurrentTask.Id == "Tarea5")
        {
            filtroAuxiliar?.ActivarGases();
        }

        if (taskManager.CurrentTask.Id == "Tarea6")
        {

        }

        if (taskManager.CurrentTask.Id == "Tarea7")
        {

        }

        if (taskManager.CurrentTask.Id == "Tarea8")
        {

        }

        taskManager?.ResolveCurrentTask(DecisionType.Reject);
        Activar(); // Reactivar el botón después de interactuar
    }

    public void Activar()
    {
        interactionText = activeInteractionText;
        canInteract = true;
    }

    public void Desactivar()
    {
        interactionText = inactiveInteractionText;
        canInteract = false;
    }

    private IEnumerator AnimarPulsado()
    {
        Vector3 objetivoPulsado = escalaOriginal * escalaPulsado;
        float mitad = duracionPulsado * 0.5f;
        float elapsed = 0f;

        while (elapsed < mitad)
        {
            elapsed += Time.deltaTime;
            visualBoton.localScale = Vector3.Lerp(escalaOriginal, objetivoPulsado, elapsed / mitad);
            yield return null;
        }

        elapsed = 0f;

        while (elapsed < mitad)
        {
            elapsed += Time.deltaTime;
            visualBoton.localScale = Vector3.Lerp(objetivoPulsado, escalaOriginal, elapsed / mitad);
            yield return null;
        }

        visualBoton.localScale = escalaOriginal;
    }
}
