using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Graphic crosshairGraphic;
    [SerializeField] private TextMeshProUGUI interactionText;

    [Header("Crosshair")]
    [SerializeField] private float normalCrosshairSize = 8f;
    [SerializeField] private float interactableCrosshairSize = 12f;
    [SerializeField, Range(0f, 1f)] private float normalOpacity = 0.45f;
    [SerializeField, Range(0f, 1f)] private float interactableOpacity = 1f;
    [SerializeField] private float transitionSpeed = 12f;

    private RectTransform crosshairRect;
    private float targetCrosshairSize;
    private float targetOpacity;

    private void Awake()
    {
        if (crosshairGraphic != null)
        {
            crosshairRect = crosshairGraphic.rectTransform;
        }

        targetCrosshairSize = normalCrosshairSize;
        targetOpacity = normalOpacity;

        ApplyInstantState();
        HideInteraction();
    }

    private void Update()
    {
        float lerpAmount = Time.unscaledDeltaTime * transitionSpeed;

        // Suaviza el cambio visual del crosshair cuando entra o sale de un interactuable.
        if (crosshairRect != null)
        {
            Vector2 targetSize = Vector2.one * targetCrosshairSize;
            crosshairRect.sizeDelta = Vector2.Lerp(crosshairRect.sizeDelta, targetSize, lerpAmount);
        }

        if (crosshairGraphic != null)
        {
            Color color = crosshairGraphic.color;
            color.a = Mathf.Lerp(color.a, targetOpacity, lerpAmount);
            crosshairGraphic.color = color;
        }
    }

    public void ShowInteraction(string text)
    {
        targetCrosshairSize = interactableCrosshairSize;
        targetOpacity = interactableOpacity;

        if (interactionText == null)
        {
            return;
        }

        interactionText.text = text;
        interactionText.gameObject.SetActive(true);
    }

    public void HideInteraction()
    {
        targetCrosshairSize = normalCrosshairSize;
        targetOpacity = normalOpacity;

        if (interactionText == null)
        {
            return;
        }

        interactionText.text = string.Empty;
        interactionText.gameObject.SetActive(false);
    }

    private void ApplyInstantState()
    {
        if (crosshairRect != null)
        {
            crosshairRect.sizeDelta = Vector2.one * normalCrosshairSize;
        }

        if (crosshairGraphic != null)
        {
            Color color = crosshairGraphic.color;
            color.a = normalOpacity;
            crosshairGraphic.color = color;
        }
    }
}
