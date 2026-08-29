using System.Collections;
using UnityEngine;

public class BotonAutorizar : MonoBehaviour, IInteractable
{
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private BotonRechazar botonRechazar;
    [SerializeField] private string activeInteractionText = "E - authorize";
    [SerializeField] private string inactiveInteractionText = "Complete the task to obtain authorization";
    [SerializeField] private string interactionText = "Complete the task to obtain authorization";
    [SerializeField] private string interactableName = "authorized button";
    [SerializeField] private bool canInteract = false;
    [SerializeField] private Animator animatorPapel;
    [SerializeField] private string nombreAnimacionPapel = "PapelAnim";
    [SerializeField] private AudioSource audioPapel;
    [SerializeField] private Transform visualBoton;
    [SerializeField] private float escalaPulsado = 0.85f;
    [SerializeField] private float duracionPulsado = 0.15f;

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
        Desactivar();
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

        taskManager?.ResolveCurrentTask(DecisionType.Authorize);
        Desactivar();
        botonRechazar?.Activar();
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
