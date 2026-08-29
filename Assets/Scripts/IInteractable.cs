public interface IInteractable
{
    string GetInteractionText();
    bool CanInteract();
    void Interact();
}

public interface IHoldInteractable
{
    float GetHoldDuration();
}

public interface IContinuousHoldInteractable
{
    void SetHoldState(bool isHeld);
}
