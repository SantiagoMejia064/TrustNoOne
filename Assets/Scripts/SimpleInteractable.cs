using UnityEngine;

public class SimpleInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionText = "E - Interactuar";
    [SerializeField] private string interactableName = "Objeto interactivo";
    [SerializeField] private bool canInteract = true;

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
    }
}
