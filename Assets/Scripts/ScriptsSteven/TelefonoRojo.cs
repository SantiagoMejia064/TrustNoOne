using UnityEngine;

public class TelefonoRojo : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionText = "E - Interactuar";
    [SerializeField] private string interactableName = "Telefono rojo";
    [SerializeField] private bool canInteract = true;

    public AudioSource audioEstatica;
    public Animator anim;

    [SerializeField] private TaskManager taskManager;

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
        if (!canInteract || anim == null)
        {
            return;
        }

        Debug.Log("Interactuando con: " + interactableName);
        anim.SetBool("isUp", !anim.GetBool("isUp"));
    }

    private void Update()
    {
        if (taskManager == null || audioEstatica == null || anim == null)
        {
            if (audioEstatica != null && audioEstatica.isPlaying)
            {
                audioEstatica.Stop();
            }

            return;
        }

        TaskData currentTask = taskManager.CurrentTask;

        if (currentTask != null && currentTask.Type == TaskType.Legitimate && anim.GetBool("isUp"))
        {
            if (!audioEstatica.isPlaying)
            {
                audioEstatica.Play();
            }
        }
        else if (audioEstatica.isPlaying)
        {
            audioEstatica.Stop();
        }
    }
}
