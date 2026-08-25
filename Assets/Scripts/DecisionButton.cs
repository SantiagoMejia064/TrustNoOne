using UnityEngine;

public class DecisionButton : MonoBehaviour, IInteractable
{
    [SerializeField] private DecisionType decisionType;
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private string authorizeText = "E - AUTORIZAR";
    [SerializeField] private string rejectText = "E - RECHAZAR";

    public string GetInteractionText()
    {
        return decisionType == DecisionType.Authorize ? authorizeText : rejectText;
    }

    public bool CanInteract()
    {
        return taskManager != null
            && taskManager.GetCurrentTask() != null
            && !taskManager.IsCurrentTaskResolved();
    }

    public void Interact()
    {
        if (taskManager == null)
        {
            Debug.LogWarning("DecisionButton no puede resolver la decision porque falta TaskManager.");
            return;
        }

        Debug.Log("Boton de decision pulsado: " + decisionType);
        taskManager.ResolveCurrentTask(decisionType);
    }
}
