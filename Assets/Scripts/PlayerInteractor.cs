using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private InteractionUI interactionUI;

    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactableLayer;

    private IInteractable currentInteractable;

    private void Update()
    {
        UpdateCurrentInteractable();
        HandleInteractionInput();
    }

    private void UpdateCurrentInteractable()
    {
        IInteractable detectedInteractable = GetInteractableInView();

        if (detectedInteractable == currentInteractable)
        {
            RefreshInteractionUI();
            return;
        }

        currentInteractable = detectedInteractable;
        RefreshInteractionUI();
    }

    private IInteractable GetInteractableInView()
    {
        if (playerCamera == null)
        {
            return null;
        }

        // El raycast sale del centro de la camara, que coincide con el crosshair.
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            return null;
        }

        MonoBehaviour[] behaviours = hit.collider.GetComponentsInParent<MonoBehaviour>();

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IInteractable interactable && IsInInteractableLayer(hit.collider.gameObject, behaviour.gameObject))
            {
                return interactable;
            }
        }

        return null;
    }

    private bool IsInInteractableLayer(GameObject hitObject, GameObject interactableObject)
    {
        return IsLayerInMask(hitObject.layer, interactableLayer)
            || IsLayerInMask(interactableObject.layer, interactableLayer)
            || HasParentInInteractableLayer(hitObject.transform);
    }

    private bool HasParentInInteractableLayer(Transform transformToCheck)
    {
        Transform current = transformToCheck.parent;

        while (current != null)
        {
            if (IsLayerInMask(current.gameObject.layer, interactableLayer))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private bool IsLayerInMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }

    private void RefreshInteractionUI()
    {
        if (interactionUI == null)
        {
            return;
        }

        if (currentInteractable != null && currentInteractable.CanInteract())
        {
            interactionUI.ShowInteraction(currentInteractable.GetInteractionText());
            return;
        }

        interactionUI.HideInteraction();
    }

    private void HandleInteractionInput()
    {
        if (!WasInteractionPressed())
        {
            return;
        }

        if (currentInteractable == null || !currentInteractable.CanInteract())
        {
            return;
        }

        currentInteractable.Interact();
    }

    private bool WasInteractionPressed()
    {
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
    }
}
