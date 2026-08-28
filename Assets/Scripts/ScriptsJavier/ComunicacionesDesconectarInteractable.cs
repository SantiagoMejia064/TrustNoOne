using UnityEngine;

/// <summary>
/// Objeto de la tarea de sabotaje "Reiniciar sistema de comunicaciones".
/// El panel muestra que Comunicaciones esta estable (es una tarea de
/// sabotaje), por lo que lo correcto es RECHAZAR sin tocar nada.
///
/// Si el jugador insiste en "obedecer" la orden falsa, primero debe
/// desconectar las comunicaciones interactuando con este objeto. Al
/// hacerlo:
/// - Se habilita el boton Autorizar (antes estaba desactivado por defecto).
/// - Se deshabilita el boton Rechazar (ya no tiene sentido rechazar algo
///   que ya se desconecto).
/// </summary>
public class ComunicacionesDesconectarInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionText = "E - Desconectar comunicaciones";
    [SerializeField] private string interactableName = "Panel de comunicaciones";
    [SerializeField] private bool canInteract = true;

    [SerializeField] private BotonAutorizar botonAutorizar;
    [SerializeField] private BotonRechazar botonRechazar;

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