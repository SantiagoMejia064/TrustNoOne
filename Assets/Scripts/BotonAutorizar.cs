using UnityEngine;

public class BotonAutorizar : MonoBehaviour, IInteractable
{
    [SerializeField] private string activeInteractionText = "E - authorize";
    [SerializeField] private string inactiveInteractionText = "Complete the task to obtain authorization";
    [SerializeField] private string interactionText = "Complete the task to obtain authorization";
    [SerializeField] private string interactableName = "authorized button";
    [SerializeField] private bool canInteract = false;

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
