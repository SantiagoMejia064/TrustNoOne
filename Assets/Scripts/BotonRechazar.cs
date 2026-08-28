using UnityEngine;

public class BotonRechazar : MonoBehaviour, IInteractable
{
    [SerializeField] private string activeInteractionText = "E - Reject";
    [SerializeField] private string inactiveInteractionText = "You did your homework, so you can't back out now";
    [SerializeField] private string interactionText = "E - Reject";
    [SerializeField] private string interactableName = "Rejected button";
    [SerializeField] private bool canInteract = true;

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
}
