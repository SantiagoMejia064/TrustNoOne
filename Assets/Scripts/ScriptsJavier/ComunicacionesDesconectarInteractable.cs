using UnityEngine;


public class ComunicacionesDesconectarInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionText = "E - Desconectar comunicaciones";
    [SerializeField] private string interactableName = "Panel de comunicaciones";
    [SerializeField] private bool canInteract = true;

    [SerializeField] private BotonAutorizar botonAutorizar;
    [SerializeField] private BotonRechazar botonRechazar;

    [Header("Sonido al desconectar")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoDesconectar;

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

        if (audioSource != null && sonidoDesconectar != null)
        {
            audioSource.PlayOneShot(sonidoDesconectar);
        }

        if (botonAutorizar != null)
        {
            botonAutorizar.Activar();
        }

        if (botonRechazar != null)
        {
            botonRechazar.Desactivar();
        }

        // Ya se desconecto, no tiene sentido volver a interactuar con esto
        // en la misma ronda.
        canInteract = false;
    }
}