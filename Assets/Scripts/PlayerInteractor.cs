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
    private IContinuousHoldInteractable currentContinuousHoldInteractable;
    private float interactionHoldTime;
    private bool holdInteractionCompleted;

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
            UpdateContinuousHoldState();
            return;
        }

        if (currentContinuousHoldInteractable != null)
        {
            currentContinuousHoldInteractable.SetHoldState(false);
            currentContinuousHoldInteractable = null;
        }

        ResetHoldInteraction();
        currentInteractable = detectedInteractable;
        currentContinuousHoldInteractable = currentInteractable as IContinuousHoldInteractable;
        RefreshInteractionUI();
        UpdateContinuousHoldState();
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
        if (Keyboard.current == null
            || currentInteractable == null
            || !currentInteractable.CanInteract())
        {
            if (currentContinuousHoldInteractable != null)
            {
                currentContinuousHoldInteractable.SetHoldState(false);
            }
            ResetHoldInteraction();
            return;
        }

        if (currentContinuousHoldInteractable != null)
        {
            UpdateContinuousHoldState();
            return;
        }

        if (currentInteractable is IHoldInteractable holdInteractable)
        {
            HandleHoldInteraction(holdInteractable);
            return;
        }

        ResetHoldInteraction();

        if (WasInteractionPressed())
        {
            currentInteractable.Interact();
        }
    }

    private void HandleHoldInteraction(IHoldInteractable holdInteractable)
    {
        if (!Keyboard.current.eKey.isPressed)
        {
            ResetHoldInteraction();
            return;
        }

        if (holdInteractionCompleted)
        {
            return;
        }

        interactionHoldTime += Time.deltaTime;

        if (interactionHoldTime < Mathf.Max(0.1f, holdInteractable.GetHoldDuration()))
        {
            return;
        }

        holdInteractionCompleted = true;
        currentInteractable.Interact();
    }

    private void ResetHoldInteraction()
    {
        interactionHoldTime = 0f;
        holdInteractionCompleted = false;
    }

    private void UpdateContinuousHoldState()
    {
        if (currentContinuousHoldInteractable == null)
        {
            return;
        }

        bool isHolding = Keyboard.current != null && Keyboard.current.eKey.isPressed;
        currentContinuousHoldInteractable.SetHoldState(isHolding);
    }

    private bool WasInteractionPressed()
    {
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
    }
}
