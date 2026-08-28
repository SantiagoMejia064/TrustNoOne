using UnityEngine;

public class BotonGeneradorInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionText = "E - Boost energy to 120%";
    [SerializeField] private string interactableName = "Generator button";
    [SerializeField] private bool canInteract = true;
    [SerializeField] private BotonAutorizar botonAutorizar;
    [SerializeField] private BotonRechazar botonRechazar;
    [SerializeField] private Light[] lucesParaApagar;

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

        foreach (Light luz in lucesParaApagar)
        {
            if (luz != null)
            {
                luz.enabled = false;
            }
        }

        RenderSettings.ambientIntensity = 0f;
        
    }
}
